CREATE TABLE Guilds (
    guild_id BIGINT PRIMARY KEY,
    guild_name NVARCHAR(255) NOT NULL,
    is_inactive BIT NOT NULL DEFAULT 0
);