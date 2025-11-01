namespace DataObjects
{
    /// <summary>
    /// Represents a configuration key-value pair with encryption metadata
    /// </summary>
    public class ConfigurationItem
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsEncrypted { get; set; }

        public ConfigurationItem() { }

        public ConfigurationItem(string key, string value, bool isEncrypted = false)
        {
            Key = key;
            Value = value;
            IsEncrypted = isEncrypted;
        }
    }
}