using DataObjects;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using LogicLayerInterfaces;
using Microsoft.Extensions.Logging;
using System.Net;

namespace LogicLayer
{
    public class SlashCommandRegistrar(DiscordSocketClient client, ILogger<ISlashCommandRegistrar> logger) : ISlashCommandRegistrar
    {
        private readonly DiscordSocketClient _client = client;
        private readonly ILogger<ISlashCommandRegistrar> _logger = logger;

        public async Task<OperationResult<IEnumerable<SocketApplicationCommand>>> GetRegisteredSlashCommandsAsync()
        {
            try
            {
                _logger.LogDebug("Retrieving registered slash commands from Discord");

                if (_client.ConnectionState != ConnectionState.Connected)
                {
                    var errorMessage = "Discord client is not connected";
                    _logger.LogError(errorMessage);
                    return OperationResult<IEnumerable<SocketApplicationCommand>>.Failure(
                        new DiscordApiException("GetRegisteredSlashCommands", errorMessage));
                }

                var commands = await _client.GetGlobalApplicationCommandsAsync();

                _logger.LogInformation("Retrieved {Count} registered slash commands", commands.Count);
                foreach (var command in commands)
                {
                    _logger.LogDebug("Command: {Name} - {Description} - {Id} - {CreatedAt}",
                        command.Name, command.Description, command.Id, command.CreatedAt);
                }

                return OperationResult<IEnumerable<SocketApplicationCommand>>.Success(commands);
            }
            catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.Unauthorized)
            {
                var errorMessage = "Unauthorized access to Discord API - check bot token";
                _logger.LogError((int)BactaLogging.LogEvent.RegisterCommand, ex, errorMessage);
                return OperationResult<IEnumerable<SocketApplicationCommand>>.Failure(
                    new DiscordApiException("GetRegisteredSlashCommands", errorMessage, ex));
            }
            catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.TooManyRequests)
            {
                var errorMessage = "Discord API rate limit exceeded";
                _logger.LogError((int)BactaLogging.LogEvent.RegisterCommand, ex, errorMessage);
                return OperationResult<IEnumerable<SocketApplicationCommand>>.Failure(
                    new DiscordApiException("GetRegisteredSlashCommands", errorMessage, ex));
            }
            catch (HttpException ex)
            {
                var errorMessage = $"Discord API error: {ex.HttpCode} - {ex.Reason}";
                _logger.LogError((int)BactaLogging.LogEvent.RegisterCommand, ex, errorMessage);
                return OperationResult<IEnumerable<SocketApplicationCommand>>.Failure(
                    new DiscordApiException("GetRegisteredSlashCommands", errorMessage, ex));
            }
            catch (TimeoutException ex)
            {
                var errorMessage = "Timeout occurred while retrieving slash commands";
                _logger.LogError((int)BactaLogging.LogEvent.RegisterCommand, ex, errorMessage);
                return OperationResult<IEnumerable<SocketApplicationCommand>>.Failure(
                    new DiscordApiException("GetRegisteredSlashCommands", errorMessage, ex));
            }
            catch (Exception ex)
            {
                var errorMessage = "Unexpected error occurred while retrieving slash commands";
                _logger.LogError((int)BactaLogging.LogEvent.RegisterCommand, ex, errorMessage);
                return OperationResult<IEnumerable<SocketApplicationCommand>>.Failure(
                    new DiscordApiException("GetRegisteredSlashCommands", errorMessage, ex));
            }
        }

        private OperationResult<IEnumerable<SlashCommandBuilder>> GetCommandsToBeRegistered()
        {
            try
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
                        .AddOption("active-time", ApplicationCommandOptionType.SubCommand, "Leaderboard for who has the most active time.", isRequired: false),
                    new SlashCommandBuilder()
                        .WithName("config")
                        .WithDescription("🔧 Manage bot configuration (Admin only)")
                        .WithDefaultMemberPermissions(GuildPermission.Administrator)
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("set")
                            .WithDescription("Set a configuration value")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption("key", ApplicationCommandOptionType.String, "Configuration key name", isRequired: true, isAutocomplete: true)
                            .AddOption("value", ApplicationCommandOptionType.String, "Configuration value", isRequired: true)
                            .AddOption("encrypt", ApplicationCommandOptionType.Boolean, "Force encryption (default: auto-detect)", isRequired: false))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("get")
                            .WithDescription("Get a configuration value")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption("key", ApplicationCommandOptionType.String, "Configuration key name", isRequired: true, isAutocomplete: true))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("list")
                            .WithDescription("List all configuration keys")
                            .WithType(ApplicationCommandOptionType.SubCommand))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("delete")
                            .WithDescription("Delete a configuration key")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption("key", ApplicationCommandOptionType.String, "Configuration key name", isRequired: true, isAutocomplete: true))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("reload")
                            .WithDescription("Reload configuration from database")
                            .WithType(ApplicationCommandOptionType.SubCommand)),
                    new SlashCommandBuilder()
                        .WithName("developer")
                        .WithDescription("👨‍💻 Manage developer access (Developer only)")
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("add")
                            .WithDescription("Add a user to the developer list")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption("user", ApplicationCommandOptionType.User, "User to add to developer list", isRequired: true))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("remove")
                            .WithDescription("Remove a user from the developer list")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption("user", ApplicationCommandOptionType.User, "User to remove from developer list", isRequired: true))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("list")
                            .WithDescription("List all developers")
                            .WithType(ApplicationCommandOptionType.SubCommand))
                };

                return OperationResult<IEnumerable<SlashCommandBuilder>>.Success(commands);
            }
            catch (Exception ex)
            {
                var errorMessage = "Failed to build slash command definitions";
                _logger.LogError((int)BactaLogging.LogEvent.RegisterCommand, ex, errorMessage);
                return OperationResult<IEnumerable<SlashCommandBuilder>>.Failure(errorMessage, ex);
            }
        }

        public async Task<OperationResult> DeleteCommandAsync(SocketApplicationCommand command)
        {
            if (command == null)
            {
                var errorMessage = "Command cannot be null";
                _logger.LogWarning(errorMessage);
                return OperationResult.Failure(errorMessage);
            }

            try
            {
                _logger.LogInformation("Deleting slash command: {CommandName} ({CommandId})", command.Name, command.Id);

                await command.DeleteAsync();

                _logger.LogInformation("Successfully deleted command: {CommandName}", command.Name);
                return OperationResult.Success();
            }
            catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
            {
                var errorMessage = $"Command '{command.Name}' not found (may have been already deleted)";
                _logger.LogWarning((int)BactaLogging.LogEvent.RegisterCommand, errorMessage);
                return OperationResult.Success(); // Consider this a success since the command is gone
            }
            catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.Unauthorized)
            {
                var errorMessage = "Unauthorized to delete slash command - check bot permissions";
                _logger.LogError((int)BactaLogging.LogEvent.RegisterCommand, ex, errorMessage);
                return OperationResult.Failure(new DiscordApiException("DeleteCommand", errorMessage, ex));
            }
            catch (HttpException ex)
            {
                var errorMessage = $"Discord API error while deleting command '{command.Name}': {ex.HttpCode} - {ex.Reason}";
                _logger.LogError((int)BactaLogging.LogEvent.RegisterCommand, ex, errorMessage);
                return OperationResult.Failure(new DiscordApiException("DeleteCommand", errorMessage, ex));
            }
            catch (Exception ex)
            {
                var errorMessage = $"Unexpected error while deleting slash command '{command.Name}'";
                _logger.LogError((int)BactaLogging.LogEvent.RegisterCommand, ex, errorMessage);
                return OperationResult.Failure(errorMessage, ex);
            }
        }

        public async Task<OperationResult> RegisterCommandsAsync()
        {
            try
            {
                _logger.LogInformation("Starting slash command registration process");

                // Validate client connection
                if (_client.ConnectionState != ConnectionState.Connected)
                {
                    var errorMessage = "Discord client is not connected";
                    _logger.LogError(errorMessage);
                    return OperationResult.Failure(new DiscordApiException("RegisterCommands", errorMessage));
                }

                // Get existing commands
                var existingCommandsResult = await GetRegisteredSlashCommandsAsync();
                if (existingCommandsResult.IsFailure)
                {
                    return OperationResult.Failure(
                        "Failed to retrieve existing commands",
                        existingCommandsResult.Exception ?? new Exception("Unknown error retrieving existing commands"));
                }

                // Get commands to register
                var commandsToRegisterResult = GetCommandsToBeRegistered();
                if (commandsToRegisterResult.IsFailure)
                {
                    return OperationResult.Failure(
                        "Failed to get command definitions",
                        commandsToRegisterResult.Exception ?? new Exception("Unknown error getting command definitions"));
                }

                var existingCommands = existingCommandsResult.Data!;
                var commandsToRegister = commandsToRegisterResult.Data!;

                // Filter out commands that already exist
                var filteredCommands = commandsToRegister.Where(newCmd =>
                    !existingCommands.Any(existingCmd => existingCmd.Name == newCmd.Name)).ToList();

                if (!filteredCommands.Any())
                {
                    _logger.LogInformation("No new commands to register");
                    return OperationResult.Success();
                }

                // Register new commands
                var successCount = 0;
                var failureCount = 0;
                var errors = new List<string>();

                foreach (var command in filteredCommands)
                {
                    try
                    {
                        _logger.LogInformation("Registering command: {CommandName}", command.Name);

                        await _client.CreateGlobalApplicationCommandAsync(command.Build());

                        _logger.LogInformation("Successfully registered command: {CommandName}", command.Name);
                        successCount++;
                    }
                    catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.BadRequest)
                    {
                        var errorMessage = $"Invalid command definition for '{command.Name}': {ex.Reason}";
                        _logger.LogError((int)BactaLogging.LogEvent.RegisterCommand, ex, errorMessage);
                        errors.Add(errorMessage);
                        failureCount++;
                    }
                    catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.TooManyRequests)
                    {
                        var errorMessage = $"Rate limited while registering command '{command.Name}'";
                        _logger.LogError((int)BactaLogging.LogEvent.RegisterCommand, ex, errorMessage);
                        errors.Add(errorMessage);
                        failureCount++;

                        // Wait before continuing
                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = $"Failed to register command '{command.Name}': {ex.Message}";
                        _logger.LogError((int)BactaLogging.LogEvent.RegisterCommand, ex, errorMessage);
                        errors.Add(errorMessage);
                        failureCount++;
                    }
                }

                _logger.LogInformation("Command registration completed. Success: {SuccessCount}, Failures: {FailureCount}",
                    successCount, failureCount);

                if (failureCount > 0)
                {
                    var errorMessage = $"Some commands failed to register: {string.Join("; ", errors)}";
                    return OperationResult.Failure(errorMessage);
                }

                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                var errorMessage = "Unexpected error during command registration";
                _logger.LogError((int)BactaLogging.LogEvent.RegisterCommand, ex, errorMessage);
                return OperationResult.Failure(new DiscordApiException("RegisterCommands", errorMessage, ex));
            }
        }

        // Legacy method for backward compatibility
        public async Task DeleteCommand(SocketApplicationCommand command)
        {
            var result = await DeleteCommandAsync(command);
            if (result.IsFailure)
            {
                _logger.LogError("Legacy DeleteCommand method failed: {Error}", result.ErrorMessage);
                // Don't throw here to maintain backward compatibility
            }
        }
    }
}
