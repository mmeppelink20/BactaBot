using Discord;
using Discord.WebSocket;
using LogicLayerInterfaces;
using LogicLayerInterfaces.CommandHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DataObjects;
using System.Collections.Concurrent;
using static DataObjects.ButtonIdContainer;

namespace LogicLayer.CommandHandlers
{
    public class ConfigCommandHandler : IConfigCommandHandler
    {
        private readonly ILogger<IConfigCommandHandler> _logger;
        private readonly IConfiguration _configuration;
        private readonly IBactaConfigurationManager _bactaConfigurationManager;
        
        // Store pagination state for config list commands
        // Key: messageId, Value: (regularKeys, currentPage)
        private static readonly ConcurrentDictionary<ulong, (List<string> regularKeys, int currentPage)> _paginationState = new();
        private const int KEYS_PER_PAGE = 10;

        public ConfigCommandHandler(
            ILogger<IConfigCommandHandler> logger,
            IConfiguration configuration,
            IBactaConfigurationManager bactaConfigurationManager)
        {
            _logger = logger;
            _configuration = configuration;
            _bactaConfigurationManager = bactaConfigurationManager;
        }

        public async Task HandleSetCommand(SocketSlashCommand command, SocketSlashCommandDataOption subCommand)
        {
            var keyOption = subCommand.Options.FirstOrDefault(x => x.Name == "key");
            var valueOption = subCommand.Options.FirstOrDefault(x => x.Name == "value");
            var encryptOption = subCommand.Options.FirstOrDefault(x => x.Name == "encrypt");

            if (keyOption?.Value == null || valueOption?.Value == null)
            {
                await command.FollowupAsync("❌ **Error:** Both key and value are required.", ephemeral: true);
                return;
            }

            var key = keyOption.Value.ToString()!;
            var value = valueOption.Value.ToString()!;
            var forceEncryption = encryptOption?.Value as bool? ?? false;

            if (string.IsNullOrWhiteSpace(key))
            {
                await command.FollowupAsync("❌ **Error:** Configuration key cannot be empty.", ephemeral: true);
                return;
            }

            bool isSensitive = ConfigurationKeys.SensitiveKeys.Contains(key);
            var willEncrypt = forceEncryption || isSensitive;

            var result = await _bactaConfigurationManager.SetConfigurationKeyValueAsync(key, value, forceEncryption);

            if (result.IsSuccess)
            {
                var encryptionStatus = willEncrypt ? "🔒 encrypted" : "📝 plaintext";
                var embed = new EmbedBuilder()
                    .WithColor(Color.Green)
                    .WithTitle("✅ Configuration Updated")
                    .AddField("Key", $"`{key}`", true)
                    .AddField("Storage", encryptionStatus, true)
                    .AddField("Status", "Successfully saved to database", false)
                    .WithFooter($"Command executed by {command.User.Username}")
                    .WithTimestamp(DateTimeOffset.UtcNow);

                if (isSensitive && !forceEncryption)
                {
                    embed.AddField("⚠️ Notice", "This key was automatically encrypted (sensitive key)", false);
                }

                await command.FollowupAsync(embed: embed.Build(), ephemeral: true);
                
                _logger.LogCritical("Configuration key '{Key}' set by {User} ({UserId})", key, command.User.Username, command.User.Id);
            }
            else
            {
                await command.FollowupAsync($"❌ **Error setting configuration:** {result.ErrorMessage}", ephemeral: true);
            }
        }

        public async Task HandleGetCommand(SocketSlashCommand command, SocketSlashCommandDataOption subCommand)
        {
            var keyOption = subCommand.Options.FirstOrDefault(x => x.Name == "key");

            if (keyOption?.Value == null)
            {
                await command.FollowupAsync("❌ **Error:** Key is required.", ephemeral: true);
                return;
            }

            var key = keyOption.Value.ToString()!;
            var result = await _bactaConfigurationManager.RetrieveConfigurationKeyValueAsync(key);

            if (result.IsSuccess)
            {
                var configItem = result.Data!;
                var isSensitive = ConfigurationKeys.IsSensitiveKey(key, configItem.IsEncrypted);
                var displayValue = isSensitive ? "***[HIDDEN - Sensitive Value]***" : $"`{configItem.Value}`";

                var embed = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithTitle("📋 Configuration Value")
                    .AddField("Key", $"`{key}`", true)
                    .AddField("Type", isSensitive ? "🔒 Sensitive" : "📝 Regular", true)
                    .AddField("Value", displayValue, false)
                    .WithFooter($"Command executed by {command.User.Username}")
                    .WithTimestamp(DateTimeOffset.UtcNow);

                await command.FollowupAsync(embed: embed.Build(), ephemeral: true);
                
                _logger.LogCritical("Configuration key '{Key}' retrieved by {User} ({UserId})", key, command.User.Username, command.User.Id);
            }
            else
            {
                await command.FollowupAsync($"❌ **Error:** {result.ErrorMessage}", ephemeral: true);
            }
        }

        public async Task HandleListCommand(SocketSlashCommand command)
        {
            var result = await _bactaConfigurationManager.RetrieveAllConfigurationKeysValuesAsync();

            if (result.IsSuccess && result.Data!.Count > 0)
            {
                var regularKeys = new List<string>();
                var sensitiveKeys = new List<string>();

                foreach (var key in result.Data.Keys.OrderBy(k => k))
                {
                    if (ConfigurationKeys.SensitiveKeys.Contains(key))
                    {
                        sensitiveKeys.Add($"🔒 `{key}`");
                    }
                    else
                    {
                        regularKeys.Add($"📝 `{key}`");
                    }
                }

                var response = await command.FollowupAsync(
                    embed: BuildConfigListEmbed(regularKeys, sensitiveKeys, 0, command.User.Username),
                    components: BuildPaginationButtons(0, regularKeys.Count),
                    ephemeral: true
                );

                // Store pagination state
                _paginationState[response.Id] = (regularKeys, 0);
                
                _logger.LogCritical("Configuration list requested by {User} ({UserId})", command.User.Username, command.User.Id);
            }
            else
            {
                await command.FollowupAsync("📋 **No configuration keys found.**", ephemeral: true);
            }
        }

        public async Task HandleConfigListPaginationAsync(SocketMessageComponent component, bool isNext)
        {
            if (!_paginationState.TryGetValue(component.Message.Id, out var state))
            {
                await component.RespondAsync("❌ **Error:** Pagination state not found. Please run `/config list` again.", ephemeral: true);
                return;
            }

            var (regularKeys, currentPage) = state;
            var totalPages = (int)Math.Ceiling(regularKeys.Count / (double)KEYS_PER_PAGE);
            
            // Calculate new page
            var newPage = isNext ? currentPage + 1 : currentPage - 1;
            
            // Ensure page is within bounds
            if (newPage < 0 || newPage >= totalPages)
            {
                await component.DeferAsync(); // Acknowledge the interaction without changes
                return;
            }

            // Update pagination state
            _paginationState[component.Message.Id] = (regularKeys, newPage);

            // Get sensitive keys (we need to fetch them again or store them)
            var result = await _bactaConfigurationManager.RetrieveAllConfigurationKeysValuesAsync();
            var sensitiveKeys = new List<string>();
            
            if (result.IsSuccess)
            {
                foreach (var key in result.Data!.Keys.OrderBy(k => k))
                {
                    if (ConfigurationKeys.SensitiveKeys.Contains(key))
                    {
                        sensitiveKeys.Add($"🔒 `{key}`");
                    }
                }
            }

            await component.UpdateAsync(msg =>
            {
                msg.Embed = BuildConfigListEmbed(regularKeys, sensitiveKeys, newPage, component.User.Username);
                msg.Components = BuildPaginationButtons(newPage, regularKeys.Count);
            });

            _logger.LogDebug("Config list pagination: page {Page}/{TotalPages} for user {User}", 
                newPage + 1, totalPages, component.User.Username);
        }

        private static Embed BuildConfigListEmbed(List<string> regularKeys, List<string> sensitiveKeys, int currentPage, string username)
        {
            var totalPages = (int)Math.Ceiling(regularKeys.Count / (double)KEYS_PER_PAGE);
            var skip = currentPage * KEYS_PER_PAGE;
            var pageKeys = regularKeys.Skip(skip).Take(KEYS_PER_PAGE).ToList();

            var embed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithTitle("📋 Configuration Keys")
                .WithDescription($"Found {regularKeys.Count + sensitiveKeys.Count} configuration keys:")
                .WithFooter($"Page {currentPage + 1}/{totalPages} • Command executed by {username}")
                .WithTimestamp(DateTimeOffset.UtcNow);

            if (pageKeys.Count > 0)
            {
                embed.AddField($"Regular Keys ({regularKeys.Count} total)", string.Join("\n", pageKeys), true);
            }

            if (sensitiveKeys.Count > 0)
            {
                embed.AddField("Sensitive Keys", string.Join("\n", sensitiveKeys), true);
            }

            return embed.Build();
        }

        private static MessageComponent BuildPaginationButtons(int currentPage, int totalRegularKeys)
        {
            var totalPages = (int)Math.Ceiling(totalRegularKeys / (double)KEYS_PER_PAGE);
            var builder = new ComponentBuilder();

            // Add Previous button
            builder.WithButton(
                ButtonId.btnConfigListPrevious.GetLabel(),
                $"{ButtonId.btnConfigListPrevious}",
                ButtonId.btnConfigListPrevious.GetStyle(),
                disabled: currentPage == 0
            );

            // Add Next button
            builder.WithButton(
                ButtonId.btnConfigListNext.GetLabel(),
                $"{ButtonId.btnConfigListNext}",
                ButtonId.btnConfigListNext.GetStyle(),
                disabled: currentPage >= totalPages - 1
            );

            return builder.Build();
        }

        public async Task HandleDeleteCommand(SocketSlashCommand command, SocketSlashCommandDataOption subCommand)
        {
            var keyOption = subCommand.Options.FirstOrDefault(x => x.Name == "key");

            if (keyOption?.Value == null)
            {
                await command.FollowupAsync("❌ **Error:** Key is required.", ephemeral: true);
                return;
            }

            var key = keyOption.Value.ToString()!;
            var isSensitive = ConfigurationKeys.SensitiveKeys.Contains(key);
            
            var result = await _bactaConfigurationManager.DeleteConfigurationKeyAsync(key);

            if (result.IsSuccess)
            {
                var embed = new EmbedBuilder()
                    .WithColor(Color.Orange)
                    .WithTitle("🗑️ Configuration Deleted")
                    .AddField("Key", $"`{key}`", true)
                    .AddField("Type", isSensitive ? "🔒 Sensitive" : "📝 Regular", true)
                    .AddField("Status", "Successfully deleted from database", false)
                    .WithFooter($"Command executed by {command.User.Username}")
                    .WithTimestamp(DateTimeOffset.UtcNow);

                if (isSensitive)
                {
                    embed.AddField("⚠️ Warning", "This was a sensitive configuration key", false);
                }

                await command.FollowupAsync(embed: embed.Build(), ephemeral: true);
                
                _logger.LogCritical("Configuration key '{Key}' deleted by {User} ({UserId})", key, command.User.Username, command.User.Id);
            }
            else
            {
                await command.FollowupAsync($"❌ **Error deleting configuration:** {result.ErrorMessage}", ephemeral: true);
            }
        }

        public async Task HandleReloadCommand(SocketSlashCommand command)
        {
            var result = await _bactaConfigurationManager.RegisterConfigurationAsync();

            if (result.IsSuccess)
            {
                var embed = new EmbedBuilder()
                    .WithColor(Color.Green)
                    .WithTitle("🔄 Configuration Reloaded")
                    .AddField("Status", "Successfully reloaded configuration from database", false)
                    .WithFooter($"Command executed by {command.User.Username}")
                    .WithTimestamp(DateTimeOffset.UtcNow);

                await command.FollowupAsync(embed: embed.Build(), ephemeral: true);
                
                _logger.LogCritical("Configuration reloaded by {User} ({UserId})", command.User.Username, command.User.Id);
            }
            else
            {
                await command.FollowupAsync($"❌ **Error reloading configuration:** {result.ErrorMessage}", ephemeral: true);
            }
        }
    }
}