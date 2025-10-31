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