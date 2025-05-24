using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using LogicLayerInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogicLayer
{
    public class SlashCommands(ILogger<ISlashCommands> logger, IConfiguration configuration, DiscordSocketClient client, CommandService commands, IGuildMessageManager messageManager, IButtons buttons, IChatGPTManager chatGPTManager) : ISlashCommands
    {

        private readonly ILogger<ISlashCommands> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly DiscordSocketClient _client = client;
        private readonly CommandService _commands = commands;
        private readonly IGuildMessageManager _messageManager = messageManager;
        private readonly IButtons _buttons = buttons;
        private readonly IChatGPTManager _chatGPTManager = chatGPTManager;

        public async Task HandleBactaCommand(SocketSlashCommand command)
        {
            try
            {
                Random rand = new();
                int n = rand.Next(102); // Generates a random number between 0 and 101

                // Determine the reply based on the random number
                var result = n switch
                {
                    0 or 1 => "bacta max win",
                    >= 2 and <= 30 => "bacta",
                    100 or 101 => "klytobacter",
                    _ => "no bacta"
                };

                // log the command name, its invoker, and the reply
                _logger.LogInformation("Command: {CommandName} - Invoked by: {Invoker} - Result: {Result}", command.Data.Name, command.User.Username, result);

                var builder = new ComponentBuilder();

                _buttons.CreateBtnDM(builder);
                _buttons.CreateBtnShare(builder);

                // send the response with the buttons
                await command.FollowupAsync(result, components: builder.Build());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while handling the Bacta command.");
                await command.RespondAsync("An error occurred while processing your command.", ephemeral: true);
            }
        }

        public async Task HandleQuestionCommand(SocketSlashCommand command)
        {
            var questionOption = command.Data.Options.FirstOrDefault(x => x.Name == "question");

            var questionValue = questionOption?.Value?.ToString() ?? "";

            var messages = await _messageManager.RetrieveDiscordMessagesByChannelIDAndMinutesAsync(command.Channel.Id, int.Parse(_configuration["MINUTES_FOR_CHAT"] ?? "60"));

            var result = await _chatGPTManager.RetrieveQuestionAboutConversationFromChatGPTAsync(questionValue, messages);

            var builder = new ComponentBuilder();

            _buttons.CreateBtnDM(builder);
            _buttons.CreateBtnShare(builder);

            await command.FollowupAsync(result, components: builder.Build());
        }

        public async Task HandleSummarizeCommand(SocketSlashCommand command)
        {
            try
            {

                if (command.Channel is not IGuildChannel guild)
                {
                    _logger.LogWarning("Command invoked by {Invoker} in a non-guild channel", command.User);
                    return; // Exit early if the channel is not a guild channel
                }

                await command.FollowupAsync("I can't do that yet...");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while handling the Summarize command.");
                await command.FollowupAsync("An error occurred while processing your command.", ephemeral: true);
            }

        }

        public async Task HandlePingCommand(SocketSlashCommand command)
        {
            var responseTime = DateTimeOffset.UtcNow - command.CreatedAt;
            var response = $"Pong! \n\n({responseTime.Milliseconds} ms)";

            await command.FollowupAsync(response, ephemeral: true, components: ResponseButtonBuilder.Build());
        }


        public async Task HandleCreditsCommand(SocketSlashCommand command)
        {
            int credits = 100;
            await command.FollowupAsync($"You have {credits} credits", ephemeral: true);
        }

        public async Task HandleLeaderboardCommand(SocketSlashCommand command)
        {

            await command.FollowupAsync("Leaderboard", ephemeral: true);
        }

        private static ComponentBuilder ResponseButtonBuilder
        {
            get
            {
                var builder = new ComponentBuilder()
                    .WithButton("DM", "btnDm", ButtonStyle.Primary)
                    .WithButton("Share", "btnShare", ButtonStyle.Success);

                return builder;
            }
        }
    }
}
