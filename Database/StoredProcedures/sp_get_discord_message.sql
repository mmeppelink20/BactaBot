CREATE PROCEDURE sp_get_discord_message
    @message_id BIGINT
AS
BEGIN
    SELECT * FROM DiscordMessages WHERE message_id = @message_id;
END;
GO