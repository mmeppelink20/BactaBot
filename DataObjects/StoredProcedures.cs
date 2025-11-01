namespace DataObjects
{
    /// <summary>
    /// Centralized stored procedure names for consistency and maintainability
    /// </summary>
    public static class StoredProcedure
    {
        // Configuration procedures
        public const string GetConfigurationByKey = "sp_get_configuration_by_key";
        public const string GetAllConfigurationValues = "sp_get_all_configuration_values";
        public const string SetConfigurationKeyValue = "sp_set_configuration_key_value";
        public const string DeleteConfigurationKey = "sp_delete_configuration_key";
        public const string UpdateConfigurationKey = "sp_update_configuration_key";

        // Discord Message procedures
        public const string InsertDiscordMessage = "sp_insert_discord_message";
        public const string BulkInsertDiscordMessages = "sp_bulk_insert_discord_messages";
        public const string DeleteDiscordMessage = "sp_delete_discord_message";
        public const string GetRecentDiscordMessages = "sp_get_recent_discord_messages";
        public const string EditDiscordMessage = "sp_edit_discord_message";

        // Guild procedures
        public const string InsertGuild = "sp_insert_guild";
        public const string InsertMultipleGuilds = "sp_insert_multiple_guilds";
        public const string DeactivateGuilds = "sp_deactivate_guilds";
        public const string DeleteGuild = "sp_delete_guild";
        public const string UpdateGuild = "sp_update_guild";
        public const string GetGuild = "sp_get_guild";
        public const string GetAllGuilds = "sp_get_all_guilds";

        // Channel procedures
        public const string InsertChannel = "sp_insert_channel";
        public const string InsertMultipleChannels = "sp_insert_multiple_channels";
        public const string DeactivateChannels = "sp_deactivate_channels";
        public const string DeleteChannel = "sp_delete_channel";
        public const string UpdateChannel = "sp_update_channel";
        public const string GetChannel = "sp_get_channel";
        public const string GetAllChannels = "sp_get_all_channels";

        // User procedures
        public const string InsertMultipleUsers = "sp_insert_multiple_users";
        public const string InsertGuildUsers = "sp_insert_guild_users";
        public const string DeactivateGuildUsers = "sp_deactivate_guild_users";
    }
}