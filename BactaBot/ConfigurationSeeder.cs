using DataObjects;
using LogicLayerInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BactaBot
{
    /// <summary>
    /// Seeds required configuration keys into the database
    /// </summary>
    public class ConfigurationSeeder(
        ILogger<ConfigurationSeeder> logger,
        IConfiguration configuration,
        IBactaConfigurationManager configManager)
    {
        private readonly ILogger<ConfigurationSeeder> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly IBactaConfigurationManager _configManager = configManager;

        /// <summary>
        /// Seeds all required configuration keys into the database
        /// </summary>
        public async Task<bool> SeedRequiredConfigurationAsync()
        {
            _logger.LogInformation("Starting configuration seeding...");

            var seededCount = 0;
            var skippedCount = 0;
            var errors = new List<string>();

            // Use centralized required keys from ConfigurationKeys
            foreach (var key in ConfigurationKeys.RequiredKeys)
            {
                try
                {
                    // Check if already exists in DB
                    var result = await _configManager.RetrieveConfigurationKeyValueAsync(key);

                    if (result.IsSuccess && result.HasData && !string.IsNullOrEmpty(result.Data.Value))
                    {
                        _logger.LogInformation("Configuration key '{Key}' already exists in database, skipping", key);
                        skippedCount++;
                        continue;
                    }

                    // Get value from configuration or prompt user
                    var value = _configuration[key];

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        _logger.LogWarning("No value found in configuration for key '{Key}'.", key);

                        // Prompt user for input
                        value = PromptForConfigurationValue(key);

                        if (string.IsNullOrWhiteSpace(value))
                        {
                            _logger.LogWarning("No value provided for key '{Key}', skipping", key);
                            continue;
                        }
                    }

                    // Validate the value using ConfigurationValidator
                    if (!ConfigurationValidator.ValidateValue(key, value))
                    {
                        _logger.LogError("Invalid value for configuration key '{Key}': {Value}", key, value);
                        errors.Add($"Invalid value for key '{key}'");
                        continue;
                    }

                    // Determine if this is a sensitive key that needs encryption
                    bool isEncrypted = ConfigurationKeys.IsSensitiveKey(key);

                    // Store value in database
                    var setResult = await _configManager.SetConfigurationKeyValueAsync(key, value, forceEncryption: isEncrypted);

                    if (setResult.IsSuccess)
                    {
                        seededCount++;
                        _logger.LogInformation("Successfully seeded configuration key: {Key} (Encrypted: {IsEncrypted})",
                            key, isEncrypted);
                    }
                    else
                    {
                        var error = $"Failed to seed key '{key}': {setResult.ErrorMessage}";
                        errors.Add(error);
                        _logger.LogError("Failed to seed configuration key '{Key}': {Error}", key, setResult.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    var error = $"Exception while seeding key '{key}': {ex.Message}";
                    errors.Add(error);
                    _logger.LogError(ex, "Exception while seeding configuration key: {Key}", key);
                }
            }

            // Summary
            _logger.LogInformation(
                "Configuration seeding completed. Seeded: {SeededCount}, Skipped: {SkippedCount}, Errors: {ErrorCount}",
                seededCount, skippedCount, errors.Count);

            if (errors.Count > 0)
            {
                _logger.LogError("Errors encountered:\n{Errors}", string.Join("\n", errors));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Lists all required keys and whether they are configured
        /// </summary>
        public void DiagnoseConfiguration()
        {
            _logger.LogInformation("=== Configuration Diagnostic ===");

            foreach (var key in ConfigurationKeys.RequiredKeys)
            {
                var value = _configuration[key];
                var isEncrypted = ConfigurationKeys.IsSensitiveKey(key);
                var status = string.IsNullOrWhiteSpace(value) ? "MISSING" : "CONFIGURED";

                string maskedValue;
                if (string.IsNullOrWhiteSpace(value))
                {
                    maskedValue = "N/A";
                }
                else if (isEncrypted)
                {
                    maskedValue = $"{value[..Math.Min(4, value.Length)]}***";
                }
                else
                {
                    maskedValue = value.Length > 50 ? $"{value[..47]}..." : value;
                }

                _logger.LogInformation("Key: {Key,-40} Status: {Status,-12} Encrypted: {IsEncrypted,-5} Value: {Value}",
                    key, status, isEncrypted, maskedValue);
            }

            _logger.LogInformation("====================================");
        }

        /// <summary>
        /// Prompts the user to enter a configuration value via console
        /// </summary>
        private static string? PromptForConfigurationValue(string key)
        {
            Console.WriteLine();
            Console.WriteLine($"╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║  CONFIGURATION REQUIRED: {key,-39}║");
            Console.WriteLine($"╚═══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            // Provide helpful context for specific keys
            var hint = GetKeyHint(key);
            if (!string.IsNullOrEmpty(hint))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"ℹ️  {hint}");
                Console.ResetColor();
                Console.WriteLine();
            }

            // Determine if input should be masked
            bool isSensitive = ConfigurationKeys.IsSensitiveKey(key);

            Console.Write($"Enter value for {key}: ");

            string? value;
            if (isSensitive)
            {
                value = ReadSensitiveInput();
            }
            else
            {
                value = Console.ReadLine();
            }

            Console.WriteLine();

            if (!string.IsNullOrWhiteSpace(value))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ Value captured (length: {value.Length} characters)");
                Console.ResetColor();

                // Confirm the value
                Console.Write("Confirm this value? (Y/n): ");
                var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();

                if (confirm == "n" || confirm == "no")
                {
                    Console.WriteLine("Value discarded. Please re-enter.");
                    return PromptForConfigurationValue(key); // Recursive retry
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("✗ No value entered");
                Console.ResetColor();
            }

            Console.WriteLine();
            return value;
        }

        /// <summary>
        /// Reads sensitive input from console with masking
        /// </summary>
        private static string? ReadSensitiveInput()
        {
            var input = new System.Text.StringBuilder();
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Backspace && input.Length > 0)
                {
                    input.Remove(input.Length - 1, 1);
                    Console.Write("\b \b"); // Erase last asterisk
                }
                else if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Backspace)
                {
                    input.Append(key.KeyChar);
                    Console.Write("*"); // Show asterisk instead of actual character
                }
            }
            while (key.Key != ConsoleKey.Enter);

            Console.WriteLine(); // Move to next line after Enter
            return input.ToString();
        }

        /// <summary>
        /// Provides helpful hints for specific configuration keys
        /// </summary>
        private static string GetKeyHint(string key)
        {
            return key switch
            {
                ConfigurationKeys.OpenAiApiKey => "OpenAI API key should start with 'sk-' (get it from https://platform.openai.com/api-keys)",
                ConfigurationKeys.DiscordToken => "Discord bot token (get it from https://discord.com/developers/applications)",
                ConfigurationKeys.DiscordTestToken => "Discord test bot token for development (get it from https://discord.com/developers/applications)",
                ConfigurationKeys.EncryptionKey => "Base64-encoded encryption key (32 bytes recommended)",
                ConfigurationKeys.EncryptionIV => "Base64-encoded initialization vector (16 bytes recommended)",
                ConfigurationKeys.AuthenticationRetryCount => "Number of times to retry Discord connection (integer > 0)",
                ConfigurationKeys.MinutesForChat => "Minutes of chat history to include in bot responses (integer > 0)",
                ConfigurationKeys.BactaBotModel => "OpenAI model name (e.g., 'gpt-4o', 'gpt-4-turbo')",
                ConfigurationKeys.BactaBotPrompt => "System prompt that defines Bacta Bot's personality and behavior",
                ConfigurationKeys.BactaBotName => "The display name for Bacta Bot",
                ConfigurationKeys.BactaCommandBactaOdds => "Percentage chance (0-100) for regular Bacta win",
                ConfigurationKeys.BactaCommandNoBactaOdds => "Percentage chance (0-100) for no Bacta",
                ConfigurationKeys.BactaCommandKlytobacterOdds => "Percentage chance (0-100) for Klytobacter",
                ConfigurationKeys.BactaCommandBactaMaxWinOdds => "Percentage chance (0-100) for max Bacta win",
                _ => "Configuration value for " + key
            };
        }
    }
}