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

        private void PrefixCommands(SocketMessage message)
        {
            if (message.Content == "!refreshConfig")
            {
                _bactaConfigurationManager.RegisterConfiguration();
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
