using DataAccessLayerInterfaces;
using DataObjects;
using LogicLayerInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogicLayer
{
    public class BactaConfigurationManager(ILogger<IBactaConfigurationManager> logger, IConfiguration configuration, IConfigurationAccessor configurationAccessor) : IBactaConfigurationManager
    {
        private ILogger<IBactaConfigurationManager> _logger = logger;
        private IConfiguration _configuration = configuration;
        private IConfigurationAccessor _configurationAccessor = configurationAccessor;

        public async Task<Dictionary<string, string>> RetrieveAllConfigurationKeysValuesAsync()
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

            try
            {
                // async db call to get all configuration key values
                keyValuePairs = await _configurationAccessor.GetAllConfigurationKeysValuesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.Configuration, ex.Message, "Failed to retrieve all configuration key values");
            }

            return keyValuePairs;
        }

        public async Task<string> RetrieveConfigurationKeyValueAsync(string key)
        {
            string value = string.Empty;

            try
            {
                value = await _configurationAccessor.GetConfigurationKeyValueAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.Configuration, ex.Message, "Failed to retrieve configuration key value");
            }

            return value;
        }

        public void AddConfigurationKey(string key, string value)
        {
            // add the key value pair to the configuration
            _configuration[key] = value;
        }

        public void RegisterConfiguration()
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

            try
            {
                // async db call to get all configuration key values
                keyValuePairs = RetrieveAllConfigurationKeysValuesAsync().Result;

                foreach (var kvp in keyValuePairs)
                {
                    _configuration[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.Configuration, ex.Message, "Failed to retrieve all configuration key values");
            }
        }

    }
}
