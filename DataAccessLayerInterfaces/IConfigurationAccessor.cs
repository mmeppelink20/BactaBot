using DataObjects;

namespace DataAccessLayerInterfaces
{
    public interface IConfigurationAccessor
    {
        Task<ConfigurationItem?> GetConfigurationItemAsync(string key);
        Task<Dictionary<string, ConfigurationItem>> GetAllConfigurationItemsAsync();
        Task<bool> SetConfigurationKeyValueAsync(string key, string value, bool isEncrypted = false);
        Task<bool> DeleteConfigurationKeyAsync(string key);
        Task<bool> UpdateConfigurationKeyAsync(string key, string oldValue, string newValue);
        
        // Legacy methods for backward compatibility
        Task<string> GetConfigurationKeyValueAsync(string key);
        Task<Dictionary<string, string>> GetAllConfigurationKeysValuesAsync();
    }
}
