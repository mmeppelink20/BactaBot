CREATE TYPE GuildUsersTableType AS TABLE
(
    guild_id BIGINT NOT NULL,
    user_id BIGINT NOT NULL,
    nickname NVARCHAR(255) NULL,
    joined_at DATETIME NOT NULL,
    is_inactive BIT NOT NULL
);
GO