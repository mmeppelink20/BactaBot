CREATE TYPE GuildsTableType AS TABLE
(
    guild_id BIGINT PRIMARY KEY,
    guild_name NVARCHAR(255)
);
GO