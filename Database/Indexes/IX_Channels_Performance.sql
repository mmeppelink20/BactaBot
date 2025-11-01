-- For guild-channel relationship queries
CREATE NONCLUSTERED INDEX IX_Channels_GuildId 
ON Channels (guild_id)
INCLUDE (channel_name, channel_type, is_inactive);