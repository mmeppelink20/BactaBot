using DataAccessLayerInterfaces;
using DataObjects;
using LogicLayerInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data.Common;

namespace LogicLayer
{
    /// <summary>
    /// Enhanced configuration manager with encryption support for sensitive data
    /// </summary>
    public class SecureConfigurationManager : IBactaConfigurationManager
    {
        private readonly ILogger<SecureConfigurationManager> _logger;
        private readonly IConfiguration _configuration;
        private readonly IConfigurationAccessor _configurationAccessor;
        private readonly IEncryptionManager _encryptionManager;

        public SecureConfigurationManager(
            ILogger<SecureConfigurationManager> logger,
            IConfiguration configuration,
            IConfigurationAccessor configurationAccessor,
            IEncryptionManager encryptionManager)
        {
            _logger = logger;
            _configuration = configuration;
            _configurationAccessor = configurationAccessor;
            _encryptionManager = encryptionManager;
        }

        public async Task<OperationResult<Dictionary<string, string>>> RetrieveAllConfigurationKeysValuesAsync()
        {
            try
            {
                _logger.LogDebug("Retrieving and decrypting all configuration key-value pairs from database");

                var keyValuePairs = await _configurationAccessor.GetAllConfigurationKeysValuesAsync();

                if (keyValuePairs == null)
                {
                    _logger.LogWarning("Configuration accessor returned null for all configuration keys");
                    return OperationResult<Dictionary<string, string>>.Success(new Dictionary<string, string>());
                }

                var decryptedPairs = new Dictionary<string, string>();
                foreach (var kvp in keyValuePairs)
                {
                    try
                    {
                        var decryptedValue = DecryptIfSensitive(kvp.Key, kvp.Value);
                        decryptedPairs[kvp.Key] = decryptedValue;
                    }
                    catch (EncryptionException ex)
                    {
                        _logger.LogError(ex, "Failed to decrypt configuration key '{Key}'. Using encrypted value as-is.", kvp.Key);
                        // Return the encrypted value as-is if decryption fails
                        decryptedPairs[kvp.Key] = kvp.Value;
                    }
                }

                _logger.LogInformation("Successfully retrieved and processed {Count} configuration key-value pairs", decryptedPairs.Count);
                return OperationResult<Dictionary<string, string>>.Success(decryptedPairs);
            }
            catch (DbException ex)
            {
                var errorMessage = "Database error occurred while retrieving all configuration key values";
                _logger.LogError((int)BactaLogging.LogEvent.Configuration, ex, errorMessage);
                return OperationResult<Dictionary<string, string>>.Failure(new DatabaseException("RetrieveAllConfigurationKeysValues", errorMessage, ex));
            }
            catch (Exception ex)
            {
                var errorMessage = "Unexpected error occurred while retrieving all configuration key values";
                _logger.LogError((int)BactaLogging.LogEvent.Configuration, ex, errorMessage);
                return OperationResult<Dictionary<string, string>>.Failure(new ConfigurationException(errorMessage, ex));
            }
        }

        public async Task<OperationResult<ConfigurationItem>> RetrieveConfigurationKeyValueAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                var errorMessage = "Configuration key cannot be null or empty";
                _logger.LogWarning(errorMessage);
                return OperationResult<ConfigurationItem>.Failure(errorMessage);
            }

            try
            {
                _logger.LogDebug("Retrieving configuration value for key: {Key}", key);

                var value = await _configurationAccessor.GetConfigurationKeyValueAsync(key);

                if (string.IsNullOrEmpty(value))
                {
                    _logger.LogWarning("Configuration key '{Key}' returned null or empty value", key);
                    return OperationResult<ConfigurationItem>.Failure($"Configuration key '{key}' not found or has empty value");
                }

                var decryptedValue = DecryptIfSensitive(key, value);

                var item = new ConfigurationItem
                {
                    Key = key,
                    Value = decryptedValue
                };

                _logger.LogDebug("Successfully retrieved and processed configuration value for key: {Key}", key);
                return OperationResult<ConfigurationItem>.Success(item);
            }
            catch (EncryptionException ex)
            {
                var errorMessage = $"Encryption error occurred while retrieving configuration key '{key}'";
                _logger.LogError((int)BactaLogging.LogEvent.Configuration, ex, errorMessage);
                return OperationResult<ConfigurationItem>.Failure(new ConfigurationException(key, errorMessage, ex));
            }
            catch (DbException ex)
            {
                var errorMessage = $"Database error occurred while retrieving configuration key '{key}'";
                _logger.LogError((int)BactaLogging.LogEvent.Configuration, ex, errorMessage);
                return OperationResult<ConfigurationItem>.Failure(new DatabaseException("RetrieveConfigurationKeyValue", errorMessage, ex));
            }
            catch (Exception ex)
            {
                var errorMessage = $"Unexpected error occurred while retrieving configuration key '{key}'";
                _logger.LogError((int)BactaLogging.LogEvent.Configuration, ex, errorMessage);
                return OperationResult<ConfigurationItem>.Failure(new ConfigurationException(key, errorMessage, ex));
            }
        }

        public async Task<OperationResult> SetConfigurationKeyValueAsync(string key, string value)
        {
            return await SetConfigurationKeyValueAsync(key, value, false);
        }

        public async Task<OperationResult> SetConfigurationKeyValueAsync(string key, string value, bool forceEncryption)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                var errorMessage = "Configuration key cannot be null or empty";
                _logger.LogWarning(errorMessage);
                return OperationResult.Failure(errorMessage);
            }

            if (value == null)
            {
                var errorMessage = "Configuration value cannot be null";
                _logger.LogWarning("Attempted to set null value for configuration key '{Key}'", key);
                return OperationResult.Failure(errorMessage);
            }

            try
            {
                _logger.LogDebug("Setting configuration key '{Key}' in database", key);

                var valueToStore = forceEncryption ? _encryptionManager.Encrypt(value) : EncryptIfSensitive(key, value);
                var success = await _configurationAccessor.SetConfigurationKeyValueAsync(key, valueToStore);

                if (!success)
                {
                    var errorMessage = $"Failed to set configuration key '{key}' in database";
                    _logger.LogWarning(errorMessage);
                    return OperationResult.Failure(errorMessage);
                }

                // Store the decrypted value in memory
                _configuration[key] = value;

                var encryptionStatus = forceEncryption || ConfigurationKeys.SensitiveKeys.Contains(key) ? "encrypted" : "plaintext";
                _logger.LogInformation("Successfully set configuration key '{Key}' in database ({EncryptionStatus}) and memory", key, encryptionStatus);
                return OperationResult.Success();
            }
            catch (EncryptionException ex)
            {
                var errorMessage = $"Encryption error occurred while setting configuration key '{key}'";
                _logger.LogError((int)BactaLogging.LogEvent.Configuration, ex, errorMessage);
                return OperationResult.Failure(new ConfigurationException(key, errorMessage, ex));
            }
            catch (DbException ex)
            {
                var errorMessage = $"Database error occurred while setting configuration key '{key}'";
                _logger.LogError((int)BactaLogging.LogEvent.Configuration, ex, errorMessage);
                return OperationResult.Failure(new DatabaseException("SetConfigurationKeyValue", errorMessage, ex));
            }
            catch (Exception ex)
            {
                var errorMessage = $"Unexpected error occurred while setting configuration key '{key}'";
                _logger.LogError((int)BactaLogging.LogEvent.Configuration, ex, errorMessage);
                return OperationResult.Failure(new ConfigurationException(key, errorMessage, ex));
            }
        }

        public async Task<OperationResult> DeleteConfigurationKeyAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                var errorMessage = "Configuration key cannot be null or empty";
                _logger.LogWarning(errorMessage);
                return OperationResult.Failure(errorMessage);
            }

            try
            {
                _logger.LogDebug("Deleting configuration key '{Key}' from database", key);

                var success = await _configurationAccessor.DeleteConfigurationKeyAsync(key);

                if (!success)
                {
                    var errorMessage = $"Failed to delete configuration key '{key}' from database";
                    _logger.LogWarning(errorMessage);
                    return OperationResult.Failure(errorMessage);
                }

                _logger.LogInformation("Successfully deleted configuration key '{Key}' from database", key);
                return OperationResult.Success();
            }
            catch (DbException ex)
            {
                var errorMessage = $"Database error occurred while deleting configuration key '{key}'";
                _logger.LogError((int)BactaLogging.LogEvent.Configuration, ex, errorMessage);
                return OperationResult.Failure(new DatabaseException("DeleteConfigurationKey", errorMessage, ex));
            }
            catch (Exception ex)
            {
                var errorMessage = $"Unexpected error occurred while deleting configuration key '{key}'";
                _logger.LogError((int)BactaLogging.LogEvent.Configuration, ex, errorMessage);
                return OperationResult.Failure(new ConfigurationException(key, errorMessage, ex));
            }
        }

        public OperationResult AddConfigurationKey(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                var errorMessage = "Configuration key cannot be null or empty";
                _logger.LogWarning(errorMessage);
                return OperationResult.Failure(errorMessage);
            }

            if (value == null)
            {
                var errorMessage = "Configuration value cannot be null";
                _logger.LogWarning("Attempted to add null value for configuration key '{Key}'", key);
                return OperationResult.Failure(errorMessage);
            }

            try
            {
                _logger.LogDebug("Adding configuration key '{Key}' to memory", key);
                _configuration[key] = value;
                _logger.LogInformation("Successfully added configuration key '{Key}' to memory", key);
                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                var errorMessage = $"Failed to add configuration key '{key}' to memory";
                _logger.LogError((int)BactaLogging.LogEvent.Configuration, ex, errorMessage);
                return OperationResult.Failure(new ConfigurationException(key, errorMessage, ex));
            }
        }

        public async Task<OperationResult> RegisterConfigurationAsync()
        {
            try
            {
                _logger.LogInformation("Starting secure configuration registration process");

                var result = await RetrieveAllConfigurationKeysValuesAsync();
                if (result.IsFailure)
                {
                    _logger.LogError("Failed to retrieve configuration from database: {Error}", result.ErrorMessage);
                    return OperationResult.Failure("Failed to retrieve configuration from database", result.Exception ?? new Exception(result.ErrorMessage));
                }

                var keyValuePairs = result.Data!;
                var successCount = 0;
                var failureCount = 0;

                foreach (var kvp in keyValuePairs)
                {
                    var addResult = AddConfigurationKey(kvp.Key, kvp.Value);
                    if (addResult.IsSuccess)
                    {
                        successCount++;
                    }
                    else
                    {
                        failureCount++;
                        _logger.LogWarning("Failed to add configuration key '{Key}': {Error}", kvp.Key, addResult.ErrorMessage);
                    }
                }

                _logger.LogInformation("Configuration registration completed. Success: {SuccessCount}, Failures: {FailureCount}",
                    successCount, failureCount);

                // Validate the configuration after loading
                var validationResult = ConfigurationValidator.ValidateConfiguration(_configuration, _logger);
                if (validationResult.IsFailure)
                {
                    _logger.LogWarning("Configuration validation failed after registration: {Error}", validationResult.ErrorMessage);
                    return OperationResult.Failure($"Configuration validation failed: {validationResult.ErrorMessage}");
                }

                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                var errorMessage = "Unexpected error during secure configuration registration";
                _logger.LogError((int)BactaLogging.LogEvent.Configuration, ex, errorMessage);
                return OperationResult.Failure(new ConfigurationException(errorMessage, ex));
            }
        }

        public void RegisterConfiguration()
        {
            try
            {
                var result = RegisterConfigurationAsync().GetAwaiter().GetResult();
                if (result.IsFailure)
                {
                    _logger.LogError("Secure configuration registration failed: {Error}", result.ErrorMessage);
                    throw new ConfigurationException("Secure configuration registration failed", result.Exception ?? new Exception(result.ErrorMessage));
                }
            }
            catch (ConfigurationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.Configuration, ex, "Unexpected error in legacy RegisterConfiguration method");
                throw new ConfigurationException("Secure configuration registration failed", ex);
            }
        }

        /// <summary>
        /// Encrypts the value if the key is marked as sensitive
        /// </summary>
        private string EncryptIfSensitive(string key, string value)
        {
            if (ConfigurationKeys.SensitiveKeys.Contains(key))
            {
                _logger.LogDebug("Encrypting sensitive configuration key: {Key}", key);
                return _encryptionManager.Encrypt(value);
            }
            return value;
        }

        /// <summary>
        /// Decrypts the value if the key is marked as sensitive and the value appears encrypted
        /// </summary>
        private string DecryptIfSensitive(string key, string value)
        {
            if (ConfigurationKeys.SensitiveKeys.Contains(key))
            {
                if (_encryptionManager.IsEncrypted(value))
                {
                    _logger.LogDebug("Decrypting sensitive configuration key: {Key}", key);
                    return _encryptionManager.Decrypt(value);
                }
                else
                {
                    _logger.LogWarning("Sensitive key '{Key}' appears to be stored in plain text", key);
                }
            }
            return value;
        }
    }

    public class EncryptionException : Exception
    {
        public EncryptionException() { }
        public EncryptionException(string message) : base(message) { }
        public EncryptionException(string message, Exception inner) : base(message, inner) { }
    }
}