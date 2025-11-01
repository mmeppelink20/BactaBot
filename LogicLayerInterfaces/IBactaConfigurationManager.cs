using DataObjects;

namespace LogicLayerInterfaces
{
    public interface IBactaConfigurationManager
    {
        Task<OperationResult<ConfigurationItem>> RetrieveConfigurationKeyValueAsync(string key);
        Task<OperationResult<Dictionary<string, string>>> RetrieveAllConfigurationKeysValuesAsync();
        OperationResult AddConfigurationKey(string key, string value);
        Task<OperationResult> RegisterConfigurationAsync();
        Task<OperationResult> SetConfigurationKeyValueAsync(string key, string value);
        Task<OperationResult> SetConfigurationKeyValueAsync(string key, string value, bool forceEncryption);
        Task<OperationResult> DeleteConfigurationKeyAsync(string key);

        // Legacy method for backward compatibility
        void RegisterConfiguration();
    }
}