CREATE TYPE UsersTableType AS TABLE
(
    user_id BIGINT PRIMARY KEY,
    username NVARCHAR(255) NOT NULL,
    is_bot BIT NOT NULL,
    avatar_url NVARCHAR(1000) NULL
);
GO

CREATE PROCEDURE sp_insert_multiple_users
    @Users UsersTableType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    -- Perform the merge operation (Insert/Update)
    MERGE INTO Users AS target
    USING @Users AS source
    ON target.user_id = source.user_id
    WHEN MATCHED AND (
        ISNULL(target.username, '') <> ISNULL(source.username, '') OR
        ISNULL(target.is_bot, 0) <> ISNULL(source.is_bot, 0) OR
        ISNULL(target.avatar_url, '') <> ISNULL(source.avatar_url, '')
    ) THEN
        UPDATE SET 
            target.username = source.username,
            target.is_bot = source.is_bot,
            target.avatar_url = source.avatar_url
    WHEN NOT MATCHED THEN
        INSERT (user_id, username, is_bot, avatar_url)
        VALUES (source.user_id, source.username, source.is_bot, source.avatar_url);

    -- Optionally, output what changed (for debugging/logging)
    -- OUTPUT $action, inserted.*, deleted.*;
END;
GO

-- Table type for guildusers table
CREATE TYPE GuildUsersTableType AS TABLE
(
    guild_id BIGINT,
    user_id BIGINT,
    nickname NVARCHAR(255) NULL,
    joined_at DATETIME NOT NULL,
    is_inactive BIT NOT NULL DEFAULT 0,
    PRIMARY KEY (guild_id, user_id)
);
GO

CREATE PROCEDURE sp_insert_guild_users
    @GuildUsers GuildUsersTableType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    -- Perform the merge operation (Insert/Update)
    MERGE INTO GuildUsers AS target
    USING @GuildUsers AS source
    ON target.guild_id = source.guild_id AND target.user_id = source.user_id
    WHEN MATCHED AND (
        (target.nickname IS DISTINCT FROM source.nickname) OR 
        (target.joined_at <> source.joined_at) OR 
        (target.is_inactive <> source.is_inactive)
    ) THEN
        UPDATE SET 
            target.nickname = source.nickname,
            target.joined_at = source.joined_at,
            target.is_inactive = source.is_inactive
    WHEN NOT MATCHED THEN
        INSERT (guild_id, user_id, nickname, joined_at, is_inactive)
        VALUES (source.guild_id, source.user_id, source.nickname, source.joined_at, source.is_inactive);
END;
GO


