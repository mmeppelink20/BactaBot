-- Delete
CREATE PROCEDURE sp_delete_discord_message
    @message_id BIGINT
AS
BEGIN
    DELETE FROM DiscordMessages WHERE message_id = @message_id;
END;