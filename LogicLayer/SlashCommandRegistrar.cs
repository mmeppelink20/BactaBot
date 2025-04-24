using DataObjects;
using Discord;
using Discord.WebSocket;
using LogicLayerInterfaces;
using Microsoft.Extensions.Logging;

namespace LogicLayer
{
    public class SlashCommandRegistrar(DiscordSocketClient client, ILogger<ISlashCommandRegistrar> logger) : ISlashCommandRegistrar
    {
        private readonly DiscordSocketClient _client = client;
        private readonly ILogger<ISlashCommandRegistrar> _logger = logger;

        public async Task<IEnumerable<SocketApplicationCommand>> GetRegisteredSlashCommandsAsync()
        {
            var commands = await _client.GetGlobalApplicationCommandsAsync();
            foreach (var command in commands)
            {
                _logger.LogInformation((int)BactaLogging.LogEvent.RegisterCommand, $"Command: {command.Name} - {command.Description} - {command.Id} - {command.CreatedAt}");
            }

            return commands;
        }

        private IEnumerable<SlashCommandBuilder> CommandsToBeRegistered()
        {

            var commands = new List<SlashCommandBuilder>
            {
                new SlashCommandBuilder()
                    .WithName("bacta")
                    .WithDescription("bacta, or no bacta... or something else..."),
                new SlashCommandBuilder()
                    .WithName("question")
                    .WithDescription("Ask Bacta Bot a question about the conversation, or a question in general.")
                    .AddOption("question", ApplicationCommandOptionType.String, "The question to ask Bacta Bot.", isRequired: true),
                new SlashCommandBuilder()
                    .WithName("summarize")
                    .WithDescription("summarizes")
                    .AddOption("conversation-history-in-minutes", ApplicationCommandOptionType.Integer, "An integer representing the number of minutes in the past you'd like to have summarized", isRequired: false),
                new SlashCommandBuilder()
                    .WithName("ping")
                    .WithDescription("Pong!"),
                new SlashCommandBuilder()
                    .WithName("credits")
                    .WithDescription("Check how many credits you have.")
                    .AddOption("user", ApplicationCommandOptionType.Mentionable, "Check how many credits another user has"),
                new SlashCommandBuilder()
                    .WithName("leaderboard")
                    .WithDescription("Displays the leaderboard for Bacta Bot.")
                    .AddOption("credits", ApplicationCommandOptionType.SubCommand, "Leaderboard for who has the most credits.", isRequired: false)
                    .AddOption("message-count", ApplicationCommandOptionType.SubCommand, "Leaderboard for who has the most messages.", isRequired: false)
            };

            return commands;
        }

        public async Task DeleteCommand(SocketApplicationCommand command)
        {
            try
            {
                await command.DeleteAsync();
                _logger.LogInformation((int)BactaLogging.LogEvent.RegisterCommand, $"Deleted command: {command.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting slash command. {command.Name} - {command.Id}");
            }
        }

        public async Task RegisterCommandsAsync()
        {
            var existingCommands = await GetRegisteredSlashCommandsAsync();

            var commandsToRegister = CommandsToBeRegistered();

            // check if the existing commands are the same as the commands to be registered and if they are remove them from commandsToRegister
            foreach (var command in existingCommands)
            {
                var commandToRegister = commandsToRegister.FirstOrDefault(c => c.Name == command.Name);
                if (commandToRegister != null)
                {
                    commandsToRegister = commandsToRegister.Where(c => c.Name != command.Name);
                }
            }

            if (!commandsToRegister.Any())
            {
                _logger.LogInformation((int)BactaLogging.LogEvent.RegisterCommand, "No new commands to register.");
            }
            else
            {
                try
                {
                    foreach (var command in commandsToRegister)
                    {
                        _logger.LogInformation((int)BactaLogging.LogEvent.RegisterCommand, $"Registering command: {command.Name}");

                        await _client.CreateGlobalApplicationCommandAsync(command.Build());

                        _logger.LogInformation((int)BactaLogging.LogEvent.RegisterCommand, $"Registered command: {command.Name}");
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError((int)BactaLogging.LogEvent.RegisterCommand, ex.Message, "Error registering slash commands.");
                }
            }
        }

    }
}
