-- For nickname lookups in message queries
CREATE NONCLUSTERED INDEX IX_GuildUsers_UserId_GuildId 
ON GuildUsers (user_id, guild_id)
INCLUDE (nickname, joined_at);

-- For guild-specific user queries
CREATE NONCLUSTERED INDEX IX_GuildUsers_GuildId 
ON GuildUsers (guild_id)
INCLUDE (user_id, nickname);