namespace LogicLayerInterfaces
{
    public interface IBactaConfigurationManager
    {
        Task<string> RetrieveConfigurationKeyValueAsync(string key);
        Task<Dictionary<string, string>> RetrieveAllConfigurationKeysValuesAsync();
        void AddConfigurationKey(string key, string value);
        void RegisterConfiguration();
    }
}
