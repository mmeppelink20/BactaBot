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