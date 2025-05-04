CREATE PROCEDURE sp_delete_discord_message
    @message_id BIGINT
AS
BEGIN
    UPDATE DiscordMessages
    SET isDeleted = 1
    WHERE message_id = @message_id;
END;
