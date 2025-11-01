namespace DataObjects
{
    /// <summary>
    /// Centralized configuration keys for Bacta Bot application
    /// </summary>
    public static class ConfigurationKeys
    {
        // Authentication and connection settings
        public const string AuthenticationRetryCount = "AUTHENTICATION_RETRY_COUNT";
        public const string DeveloperUserIdList = "DEVELOPER_USERID_LIST";

        // Chat and messaging settings
        public const string MinutesForChat = "MINUTES_FOR_CHAT";
        public const string MinMessagesForChat = "MIN_MESSAGES_FOR_CHAT";
        public const string CharacterLimit = "CHARACTER_LIMIT";
        public const string UseCleanContent = "USE_CLEAN_CONTENT";
        public const string MentionUserOnReply = "MENTION_USER_ON_REPLY";
        public const string LogBactaPrompt = "LOG_BACTA_PROMPT";

        // Bacta command configuration
        public const string BactaCommandNoBactaOdds = "BACTA_COMMAND_NO_BACTA_ODDS";
        public const string BactaCommandNoBactaWinMessage = "BACTA_COMMAND_NO_BACTA_WIN_MESSAGE";
        public const string BactaCommandBactaOdds = "BACTA_COMMAND_BACTA_ODDS";
        public const string BactaCommandBactaWinMessage = "BACTA_COMMAND_BACTA_WIN_MESSAGE";
        public const string BactaCommandKlytobacterOdds = "BACTA_COMMAND_KLYTOBACTER_ODDS";
        public const string BactaCommandKlytobacterWinMessage = "BACTA_COMMAND_KLYTOBACTER_WIN_MESSAGE";
        public const string BactaCommandBactaMaxWinOdds = "BACTA_COMMAND_BACTA_MAX_WIN_ODDS";
        public const string BactaCommandBactaMaxWinMessage = "BACTA_COMMAND_BACTA_MAX_WIN_MESSAGE";

        // Credits system
        public const string CreditsCostPerBactaCommand = "CREDITS_COST_PER_BACTA_COMMAND";
        public const string CreditsPerBacta = "CREDITS_PER_BACTA";
        public const string CreditsPerBactaMaxWin = "CREDITS_PER_BACTA_MAX_WIN";
        public const string CreditsPerKlytobacter = "CREDITS_PER_KLYTOBACTER";
        public const string CreditsPerMessage = "CREDITS_PER_MESSAGE";
        public const string CreditsPerNoBacta = "CREDITS_PER_NO_BACTA";

        // AI and ChatGPT settings
        public const string SummaryModel = "SUMMARY_MODEL";
        public const string SummaryPrompt = "SUMMARY_PROMPT";
        public const string BactaBotModel = "BACTA_BOT_MODEL";
        public const string BactaBotPrompt = "BACTA_BOT_PROMPT";
        public const string QuestionPrompt = "QUESTION_PROMPT";
        public const string BactaBotName = "BACTA_BOT_NAME";
        public const string OpenAiApiKey = "OPEN_AI_API_KEY";

        // Discord settings
        public const string DiscordToken = "DISCORD_TOKEN";
        public const string DiscordTestToken = "DISCORD_TEST_TOKEN";

        // Encryption settings
        public const string EncryptionKey = "ENCRYPTION_KEY";
        public const string EncryptionIV = "ENCRYPTION_IV";

        // Logging settings
        public const string LoggingDefault = "Logging:Default";

        /// <summary>
        /// Configuration keys that should be encrypted when stored in the database
        /// </summary>
        public static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            OpenAiApiKey,
            DiscordToken,
            DiscordTestToken,
            EncryptionKey,
            EncryptionIV
        };

        /// <summary>
        /// Configuration keys that are required for the bot to function
        /// </summary>
        public static readonly HashSet<string> RequiredKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            AuthenticationRetryCount,
            MinutesForChat,
            BactaBotModel,
            BactaBotPrompt,
            BactaBotName,
#if DEBUG
            DiscordTestToken,
#else
            DiscordToken,
#endif
            OpenAiApiKey
        };

        /// <summary>
        /// Determines if a configuration key should be treated as sensitive based on database flag or fallback logic
        /// </summary>
        /// <param name="key">The configuration key to check</param>
        /// <param name="isEncryptedInDb">Whether the key is marked as encrypted in the database</param>
        /// <param name="forceEncryption">Whether encryption was explicitly requested</param>
        /// <returns>True if the key should be treated as sensitive</returns>
        public static bool IsSensitiveKey(string key, bool? isEncryptedInDb = null, bool forceEncryption = false)
        {
            // If database encryption flag is available, use it
            if (isEncryptedInDb.HasValue)
                return isEncryptedInDb.Value;

            // Otherwise fall back to existing logic
            return forceEncryption || SensitiveKeys.Contains(key);
        }

        /// <summary>
        /// Determines if a configuration key is required for bot operation
        /// </summary>
        /// <param name="key">The configuration key to check</param>
        /// <returns>True if the key is required</returns>
        public static bool IsRequiredKey(string key)
        {
            return RequiredKeys.Contains(key);
        }
    }
}