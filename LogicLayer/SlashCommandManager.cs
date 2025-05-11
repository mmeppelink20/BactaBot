using Discord.Commands;
using Discord.WebSocket;
using LogicLayerInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogicLayer
{
    public class SlashCommandManager : ISlashCommandManager
    {
        private readonly ILogger<ISlashCommandManager> _logger;
        private readonly IConfiguration _configuration;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IGuildMessageManager _messageManager;
        private readonly ISlashCommandRegistrar _slashCommandRegistrar;
        private readonly ISlashCommands _slashCommands;

        private readonly Dictionary<string, Func<SocketSlashCommand, Task>> _slashCommandList;
        private readonly HashSet<string> _ephemeralCommands;

        public SlashCommandManager(
            ILogger<ISlashCommandManager> logger,
            IConfiguration configuration,
            DiscordSocketClient client,
            CommandService commands,
            IGuildMessageManager messageManager,
            ISlashCommandRegistrar slashCommandRegistrar,
            ISlashCommands slashCommands)
        {
            _logger = logger;
            _configuration = configuration;
            _client = client;
            _commands = commands;
            _messageManager = messageManager;
            _slashCommandRegistrar = slashCommandRegistrar;
            _slashCommands = slashCommands;

            _slashCommandList = new()
            {
                { "bacta", slashCommands.HandleBactaCommand },
                { "question", slashCommands.HandleQuestionCommand },
                { "summarize", slashCommands.HandleSummarizeCommand },
                { "ping", slashCommands.HandlePingCommand },
                { "credits", slashCommands.HandleCreditsCommand },
                { "leaderboard", slashCommands.HandleLeaderboardCommand }
            };

            _ephemeralCommands = ["bacta", "question", "summarize", "ping", "credits", "leaderboard"];
        }

        public async Task CommandExecutorAsync(SocketSlashCommand command)
        {
            // start timer
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            if (_slashCommandList.TryGetValue(command.Data.Name, out var handler))
            {
                await command.DeferAsync(ephemeral: _ephemeralCommands.Contains(command.Data.Name));
                await handler(command);
            }
            else
            {
                await command.RespondAsync("Unknown command!", ephemeral: true);
            }
        }
    }
}