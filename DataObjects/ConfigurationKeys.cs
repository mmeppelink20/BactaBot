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
        public const string DiscordToken = "profiles:BactaBot:environmentVariables:DISCORD_TOKEN";
        public const string DiscordTestToken = "profiles:BactaBot:environmentVariables:DISCORD_TEST_TOKEN";

        // Logging settings
        public const string LoggingDefault = "Logging:Default";
    }
}