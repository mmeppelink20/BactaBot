ALTER PROCEDURE sp_delete_discord_message
    @message_id BIGINT
    @message_deleted_datetime DATETIME = NULL
AS
BEGIN
    UPDATE DiscordMessages
    SET isDeleted = 1,
        deleted_at = @message_deleted_datetime
    WHERE message_id = @message_id;
END;
