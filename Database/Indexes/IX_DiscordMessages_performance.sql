-- Critical performance indexes for DiscordMessages table

-- For sp_get_recent_discord_messages (filtered by channel and time)
CREATE NONCLUSTERED INDEX IX_DiscordMessages_ChannelId_MessageDatetime 
ON DiscordMessages (channel_id, message_datetime DESC)
INCLUDE (user_id, content, clean_content, isEdited, message_edited_datetime, attachment_url, message_link, replied_to_message_id, isDeleted, message_deleted_datetime);

-- For time-based queries
CREATE NONCLUSTERED INDEX IX_DiscordMessages_MessageDatetime 
ON DiscordMessages (message_datetime DESC);

-- For user-specific message searches
CREATE NONCLUSTERED INDEX IX_DiscordMessages_UserId_MessageDatetime 
ON DiscordMessages (user_id, message_datetime DESC);