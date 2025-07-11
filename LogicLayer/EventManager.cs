using DataObjects;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using LogicLayerInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System.Collections.Immutable;

namespace LogicLayer
{
    public class EventManager(ILogger<IEventManager> logger,
        IConfiguration configuration,
        DiscordSocketClient client,
        CommandService commands,
        IGuildMessageManager messageManager,
        ISlashCommandManager slashCommandManager,
        ISlashCommandRegistrar slashCommandRegistrar,
        IButtonManager buttonManager,
        IBactaConfigurationManager bactaConfigurationManager,
        IChatGPTManager chatGPTManager) : IEventManager
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
                // if the message is not from a guild channel, return
                if (message.Flags.HasValue && !message.Flags.Value.HasFlag(MessageFlags.Ephemeral) && message.Channel is IGuildChannel guildChannel)
                {

                    await _messageManager.AddDiscordMessageAsync(message);

                    if (message.Author.IsBot /*|| message.Author.IsWebhook*/)
                    {
                        return;
                    }

                    // check if the bot is mentioned in the message
                    if (message.MentionedUsers.Any(user => user.Id == _client.CurrentUser.Id))
                    {
                        BactaBotMentioned(message);
                    }

                    PrefixCommands(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.AddGuildMessage, ex, "Channel is not a guild channel");
            }

            await Task.CompletedTask;
        }

        public async Task MessageDeleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
        {

            if (channel.Value is not IGuildChannel guildChannel)
            {
                return;
            }

            bool isDeleted = false;

            try
            {
                isDeleted = await _messageManager.DeleteDiscordMessageAsync(message.Id);

                if (isDeleted)
                {
                    _logger.LogTrace((int)BactaLogging.LogEvent.MessageRelated, "Message deleted from database: {message}", message.Id);
                }
                else
                {
                    _logger.LogWarning((int)BactaLogging.LogEvent.MessageRelated, "Message deletion failed or message not found in database: {message}", message.Id);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.MessageRelated, ex, "An error occurred while deleting the guild message");
            }
        }

        public Task MessageUpdated(SocketMessage oldMessage, SocketMessage newMessage)
        {
            throw new NotImplementedException();
        }

        private async void PrefixCommands(SocketMessage message)
        {
            var developerUserIds = ParseDeveloperIDList();

            if ((message.Author.Id == _client.CurrentUser.Id || developerUserIds.Contains(message.Author.Id)))
            {
                string prefixCommandInvoked = string.Empty;

                if (message.Content == "!refreshConfig")
                {
                    prefixCommandInvoked = "refreshConfig";

                    _logger.LogCritical("Refresh config requested by {User}: {UserID}", message.Author.Username, message.Author.Id);

                    await MessageDevelopers(message, prefixCommandInvoked);

                    _bactaConfigurationManager.RegisterConfiguration();

                }
                else if (message.Content == "!shutdown")
                {
                    if (ShutdownRequested != null)
                    {
                        prefixCommandInvoked = "shutdown";

                        _logger.LogCritical("Shutdown requested by {User}: {UserID}", message.Author.Username, message.Author.Id);

                        await MessageDevelopers(message, prefixCommandInvoked);

                        await ShutdownRequested();
                    }
                }
            }

        }

        private HashSet<ulong> ParseDeveloperIDList()
        {
            var developerUserIdList = _configuration["DEVELOPER_USERID_LIST"];
            if (string.IsNullOrEmpty(developerUserIdList))
            {
                _logger.LogWarning("DEVELOPER_USERID_LIST is not configured or is empty.");
                return new HashSet<ulong>();
            }
            return new HashSet<ulong>(developerUserIdList.Split(',').Select(ulong.Parse));
        }

        private async Task MessageDevelopers(SocketMessage message, string parameter)
        {
            var developerUserIds = ParseDeveloperIDList();

            foreach (var userId in developerUserIds)
            {
                var user = _client.GetUser(userId);

                if (user != null)
                {
                    await user.SendMessageAsync($"{parameter}: {message.Author.Mention} ({message.Author.Id}) \n\n" + $"Location: {message.GetJumpUrl()}");
                }
            }
        }

        private async void BactaBotMentioned(SocketMessage message)
        {
            if (message is not SocketUserMessage userMessage)
            {
                return;
            }

            if (message.Reference?.MessageId.IsSpecified == true)
            {
                try
                {
                    var referencedMessage = await message.Channel.GetMessageAsync(message.Reference.MessageId.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning((int)BactaLogging.LogEvent.MessageRelated, ex, "Failed to retrieve referenced message.");
                }
            }

            try
            {
                await message.Channel.TriggerTypingAsync();

                var response = await _chatGPTManager.RetrieveChatBotCompletionFromChatGPTAsync(message, int.Parse(_configuration["MINUTES_FOR_CHAT"] ?? "60"));

                var allowedMentions = _configuration["MENTION_USER_ON_REPLY"]?.ToLower() == "0" ? AllowedMentions.None : null;

                await userMessage.ReplyAsync(response, allowedMentions: allowedMentions);
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.ChatGPT, ex, "An error occurred while retrieving the chat bot completion.");

                await MessageDevelopers(message, ex.ToString());
                await userMessage.ReplyAsync("An error occurred while processing your message. Please try again later.");
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
