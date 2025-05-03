using DataObjects;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LogicLayerInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogicLayer
{
    public class EventManager(ILogger<IEventManager> logger, IConfiguration configuration, DiscordSocketClient client, CommandService commands, IGuildMessageManager messageManager, ISlashCommandManager slashCommandManager, ISlashCommandRegistrar slashCommandRegistrar, IButtonManager buttonManager, IBactaConfigurationManager bactaConfigurationManager, IChatGPTManager chatGPTManager) : IEventManager
    {
        private readonly ILogger<IEventManager> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly DiscordSocketClient _client = client;
        private readonly CommandService _commands = commands;
        private readonly IGuildMessageManager _messageManager = messageManager;
        private readonly ISlashCommandManager _slashCommandManager = slashCommandManager;
        private readonly ISlashCommandRegistrar _slashCommandRegistrar = slashCommandRegistrar;
        private readonly IButtonManager _buttonManager = buttonManager;
        private readonly IBactaConfigurationManager _bactaConfigurationManager = bactaConfigurationManager;
        private readonly IChatGPTManager _chatGPTManager = chatGPTManager;

        public Func<Task>? ShutdownRequested { get; set; }

        public async Task MessageRecieved(SocketMessage message)
        {
            try
            {
                if (message.Flags.HasValue && !message.Flags.Value.HasFlag(MessageFlags.Ephemeral))
                {
                    await _messageManager.AddDiscordMessageAsync(message);
                }

                PrefixCommands(message);
                BactaBotMentioned(message);
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.AddGuildMessage, ex, "Channel is not a guild channel");
            }

            await Task.CompletedTask;
        }

        private async void PrefixCommands(SocketMessage message)
        {
            // Ensure the configuration value is not null or empty before splitting  
            var developerUserIdList = _configuration["DEVELOPER_USERID_LIST"];
            if (!string.IsNullOrEmpty(developerUserIdList))
            {
                var developerUserIds = new HashSet<ulong>(developerUserIdList.Split(',').Select(ulong.Parse));
                if ((message.Author.Id == _client.CurrentUser.Id || developerUserIds.Contains(message.Author.Id)))
                {

                    if (message.Content == "!refreshConfig")
                    {
                        _bactaConfigurationManager.RegisterConfiguration();
                    }
                    else if (message.Content == "!shutdown")
                    {
                        if (ShutdownRequested != null)
                        {
                            _logger.LogCritical("Shutdown requested by {User}: {UserID}", message.Author.Username, message.Author.Id);
                            // send a DM to all users in developerUserIdList that the bot is shutting down and where it was shut down and by who
                            foreach (var userId in developerUserIds)
                            {
                                var user = _client.GetUser(userId);
                                if (user != null)
                                {
                                    await user.SendMessageAsync($"The bot is shutting down. \n\n" +
                                        $"Shut down by: {message.Author.Mention} ({message.Author.Id}) \n\n" +
                                        $"Location: {message.GetJumpUrl()}");
                                }
                            }

                            await ShutdownRequested();
                        }
                    }

                }
            }
            else
            {
                _logger.LogWarning("DEVELOPER_USERID_LIST is not configured or is empty.");
                return;
            }

        }

        private async void BactaBotMentioned(SocketMessage message)
        {
            // if the message contains a mention of the bot
            if (message.Content.Contains(_client.CurrentUser.Mention))
            {
                // start typing
                await message.Channel.TriggerTypingAsync();

                if (message is SocketUserMessage userMessage)
                {
                    var response = await _chatGPTManager.RetrieveChatBotCompletionFromChatGPTAsync(message, int.Parse(_configuration["MINUTES_FOR_CHAT"] ?? "60"));

                    await userMessage.ReplyAsync(response);
                }
            }
        }

        public async Task SlashCommandExecuted(SocketSlashCommand command)
        {
            await _slashCommandManager.CommandExecutorAsync(command);
        }

        public async Task ButtonExecuted(SocketMessageComponent component)
        {
            await _buttonManager.ButtonExecutorAsync(component);
        }
    }
}
