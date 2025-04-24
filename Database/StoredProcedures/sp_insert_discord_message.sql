CREATE PROCEDURE sp_insert_discord_message
    @message_id BIGINT,
    @channel_id BIGINT,
    @user_id BIGINT,
    @content NVARCHAR(2000),
    @clean_content NVARCHAR(2000),
    @message_datetime DATETIME,
    @isEdited BIT = 0,
    @message_edited_datetime DATETIME = NULL,
    @attachment_url NVARCHAR(MAX) = NULL,
    @message_link NVARCHAR(MAX) = NULL,
    @replied_to_message_id BIGINT = NULL,
    @channel_name NVARCHAR(255),
    @channel_type NVARCHAR(255),
    @guild_id BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- Insert the channel type if it does not exist
    IF NOT EXISTS (SELECT 1 FROM ChannelTypes WHERE channel_type = @channel_type)
    BEGIN
        INSERT INTO ChannelTypes (channel_type)
        VALUES (@channel_type);
    END;

    -- Insert the channel if it does not exist
    IF NOT EXISTS (SELECT 1 FROM Channels WHERE channel_id = @channel_id)
    BEGIN
        INSERT INTO Channels (channel_id, channel_name, channel_type, guild_id)
        VALUES (@channel_id, @channel_name, @channel_type, @guild_id);
    END;

    -- Insert the message
    INSERT INTO DiscordMessages (message_id, channel_id, user_id, content, clean_content, message_datetime, isEdited, message_edited_datetime, attachment_url, message_link, replied_to_message_id)
    VALUES (@message_id, @channel_id, @user_id, @content, @clean_content, @message_datetime, @isEdited, @message_edited_datetime, @attachment_url, @message_link, @replied_to_message_id);

    RETURN 0; -- Success
END;