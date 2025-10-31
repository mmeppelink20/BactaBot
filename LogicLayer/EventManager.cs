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
                else if (message.Content == "!importHistory")
                {
                    prefixCommandInvoked = "importHistory";
                    _logger.LogCritical("Import history requested by {User}: {UserID}", message.Author.Username, message.Author.Id);
                    await MessageDevelopers(message, prefixCommandInvoked);
                    await ImportChannelHistoryAsync(message);
                }

            }

        }

        private HashSet<ulong> ParseDeveloperIDList()
        {
            var developerUserIdList = _configuration[ConfigurationKeys.DeveloperUserIdList];
            if (string.IsNullOrEmpty(developerUserIdList))
            {
                _logger.LogWarning($"{ConfigurationKeys.DeveloperUserIdList} is not configured or is empty.");
                return [];
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

            try
            {
                await message.Channel.TriggerTypingAsync();

                // Add retry logic with exponential backoff
                int maxRetries = 3;
                int currentRetry = 0;
                while (currentRetry < maxRetries)
                {
                    try
                    {
                        var response = await _chatGPTManager.RetrieveChatBotCompletionFromChatGPTAsync(
                            message, 
                            int.Parse(_configuration[ConfigurationKeys.MinutesForChat] ?? "60")
                        );

                        var allowedMentions = _configuration[ConfigurationKeys.MentionUserOnReply]?.ToLower() == "0" 
                            ? AllowedMentions.None 
                            : null;

                        await userMessage.ReplyAsync(response, allowedMentions: allowedMentions);
                        return; // Success, exit the method
                    }
                    catch (HttpRequestException ex) when (ex.InnerException is IOException)
                    {
                        currentRetry++;
                        if (currentRetry >= maxRetries)
                            throw; // Rethrow if we've exhausted retries

                        // Exponential backoff
                        int delayMs = (int)Math.Pow(2, currentRetry) * 1000;
                        await Task.Delay(delayMs);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.ChatGPT, ex, 
                    "An error occurred while retrieving the chat bot completion. Retry count exceeded.");

                await MessageDevelopers(message, ex.ToString());
                await userMessage.ReplyAsync(
                    "I'm having trouble processing your message right now. Please try again in a few moments.");
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

        private async Task ImportChannelHistoryAsync(SocketMessage message)
        {
            if (message.Channel is not SocketTextChannel textChannel)
            {
                _logger.LogWarning((int)BactaLogging.LogEvent.MessageRelated, "Channel is not a text channel.");
                return;
            }
            int totalImported = 0;
            int batchSize = 100;
            ulong? lastMessageId = message.Id;
            bool continueFetching = true;
            while (continueFetching)
            {
                var messages = lastMessageId == null
                    ? await textChannel.GetMessagesAsync(batchSize).FlattenAsync()
                    : await textChannel.GetMessagesAsync(lastMessageId.Value, Direction.Before, batchSize).FlattenAsync();

                var messageList = messages.ToList();

                if (messageList.Count == 0)
                {
                    break;
                }

                await _messageManager.AddDiscordMessagesAsync(messageList);

                totalImported += messageList.Count;

                lastMessageId = messageList.Last().Id;

                if (messageList.Count < batchSize)
                {
                    continueFetching = false;
                }

            }

            await message.Channel.SendMessageAsync($"Imported {totalImported} messages from this channel.");
        }
    }
}
