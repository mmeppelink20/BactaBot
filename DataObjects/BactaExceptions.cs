namespace DataObjects
{
    /// <summary>
    /// Base exception for all Bacta Bot specific exceptions
    /// </summary>
    public abstract class BactaBotException : Exception
    {
        protected BactaBotException(string message) : base(message) { }
        protected BactaBotException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when configuration operations fail
    /// </summary>
    public class ConfigurationException : BactaBotException
    {
        public string? ConfigurationKey { get; }

        public ConfigurationException(string message) : base(message) { }
        public ConfigurationException(string message, Exception innerException) : base(message, innerException) { }
        public ConfigurationException(string configurationKey, string message) : base(message)
        {
            ConfigurationKey = configurationKey;
        }
        public ConfigurationException(string configurationKey, string message, Exception innerException) : base(message, innerException)
        {
            ConfigurationKey = configurationKey;
        }
    }

    /// <summary>
    /// Exception thrown when Discord API operations fail
    /// </summary>
    public class DiscordApiException : BactaBotException
    {
        public string? Operation { get; }

        public DiscordApiException(string operation, string message) : base(message)
        {
            Operation = operation;
        }
        public DiscordApiException(string operation, string message, Exception innerException) : base(message, innerException)
        {
            Operation = operation;
        }
    }

    /// <summary>
    /// Exception thrown when database operations fail
    /// </summary>
    public class DatabaseException : BactaBotException
    {
        public string? Operation { get; }

        public DatabaseException(string operation, string message) : base(message)
        {
            Operation = operation;
        }
        public DatabaseException(string operation, string message, Exception innerException) : base(message, innerException)
        {
            Operation = operation;
        }
    }

    /// <summary>
    /// Exception thrown when external API operations fail (e.g., ChatGPT)
    /// </summary>
    public class ExternalApiException : BactaBotException
    {
        public string? ApiName { get; }
        public int? StatusCode { get; }

        public ExternalApiException(string apiName, string message) : base(message)
        {
            ApiName = apiName;
        }
        public ExternalApiException(string apiName, string message, Exception innerException) : base(message, innerException)
        {
            ApiName = apiName;
        }
        public ExternalApiException(string apiName, int statusCode, string message) : base(message)
        {
            ApiName = apiName;
            StatusCode = statusCode;
        }
    }
}