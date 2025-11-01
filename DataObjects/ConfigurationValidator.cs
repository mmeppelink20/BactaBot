using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DataObjects
{
    /// <summary>
    /// Validates required configuration keys and their values
    /// </summary>
    public static class ConfigurationValidator
    {
        private static readonly Dictionary<string, Func<string, bool>> KeyValidators = new()
        {
            { ConfigurationKeys.AuthenticationRetryCount, value => int.TryParse(value, out var result) && result > 0 },
            { ConfigurationKeys.MinutesForChat, value => int.TryParse(value, out var result) && result > 0 },
            { ConfigurationKeys.MinMessagesForChat, value => int.TryParse(value, out var result) && result >= 0 },
            { ConfigurationKeys.CharacterLimit, value => int.TryParse(value, out var result) && result > 0 },
            { ConfigurationKeys.BactaCommandNoBactaOdds, value => int.TryParse(value, out var result) && result >= 0 && result <= 100 },
            { ConfigurationKeys.BactaCommandBactaOdds, value => int.TryParse(value, out var result) && result >= 0 && result <= 100 },
            { ConfigurationKeys.BactaCommandKlytobacterOdds, value => int.TryParse(value, out var result) && result >= 0 && result <= 100 },
            { ConfigurationKeys.BactaCommandBactaMaxWinOdds, value => int.TryParse(value, out var result) && result >= 0 && result <= 100 },
            { ConfigurationKeys.OpenAiApiKey, value => !string.IsNullOrWhiteSpace(value) && value.StartsWith("sk-") },
            { ConfigurationKeys.BactaBotName, value => !string.IsNullOrWhiteSpace(value) },
            { ConfigurationKeys.BactaBotModel, value => !string.IsNullOrWhiteSpace(value) },
            { ConfigurationKeys.SummaryModel, value => !string.IsNullOrWhiteSpace(value) }
        };

        public static OperationResult ValidateConfiguration(IConfiguration configuration, ILogger? logger = null)
        {
            var errors = new List<string>();

            // Check required keys (now from ConfigurationKeys.RequiredKeys)
            foreach (var key in ConfigurationKeys.RequiredKeys)
            {
                var value = configuration[key];
                if (string.IsNullOrWhiteSpace(value))
                {
                    errors.Add($"Required configuration key '{key}' is missing or empty");
                    logger?.LogError("Required configuration key '{Key}' is missing or empty", key);
                }
            }

            // Validate key formats
            foreach (var (key, validator) in KeyValidators)
            {
                var value = configuration[key];
                if (!string.IsNullOrWhiteSpace(value) && !validator(value))
                {
                    errors.Add($"Configuration key '{key}' has invalid value: '{value}'");
                    logger?.LogError("Configuration key '{Key}' has invalid value: '{Value}'", key, value);
                }
            }

            if (errors.Count > 0)
            {
                var errorMessage = string.Join("; ", errors);
                return OperationResult.Failure($"Configuration validation failed: {errorMessage}");
            }

            logger?.LogInformation("Configuration validation passed successfully");
            return OperationResult.Success();
        }

        public static OperationResult<T> GetRequiredValue<T>(IConfiguration configuration, string key, Func<string, T> converter)
        {
            try
            {
                var value = configuration[key];
                if (string.IsNullOrWhiteSpace(value))
                {
                    return OperationResult<T>.Failure($"Required configuration key '{key}' is missing or empty");
                }

                var convertedValue = converter(value);
                return OperationResult<T>.Success(convertedValue);
            }
            catch (Exception ex)
            {
                return OperationResult<T>.Failure($"Failed to convert configuration key '{key}' to type {typeof(T).Name}", ex);
            }
        }

        public static T GetValueOrDefault<T>(IConfiguration configuration, string key, T defaultValue, Func<string, T> converter)
        {
            try
            {
                var value = configuration[key];
                return string.IsNullOrWhiteSpace(value) ? defaultValue : converter(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets the validator function for a specific configuration key
        /// </summary>
        /// <param name="key">The configuration key</param>
        /// <returns>Validator function if one exists, null otherwise</returns>
        public static Func<string, bool>? GetValidator(string key)
        {
            return KeyValidators.TryGetValue(key, out var validator) ? validator : null;
        }

        /// <summary>
        /// Validates a single configuration value
        /// </summary>
        /// <param name="key">The configuration key</param>
        /// <param name="value">The value to validate</param>
        /// <returns>True if valid or no validator exists, false otherwise</returns>
        public static bool ValidateValue(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var validator = GetValidator(key);
            return validator == null || validator(value);
        }
    }
}