namespace DataAccessLayerInterfaces
{
    public interface IConfigurationAccessor
    {
        Task<string> GetConfigurationKeyValueAsync(string key);
        Task<Dictionary<string, string>> GetAllConfigurationKeysValuesAsync();

    }
}
