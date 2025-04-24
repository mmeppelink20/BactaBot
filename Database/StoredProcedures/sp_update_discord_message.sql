CREATE PROCEDURE sp_edit_discord_message
    @message_id BIGINT,
    @old_content NVARCHAR(2000),
    @old_clean_content NVARCHAR(2000),
    @new_content NVARCHAR(2000),
    @new_clean_content NVARCHAR(2000),
    @attachment_url NVARCHAR(MAX) = NULL,
    @message_link NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @current_content NVARCHAR(2000);
    DECLARE @current_clean_content NVARCHAR(2000);

    -- Retrieve the current content of the message
    SELECT @current_content = content, 
           @current_clean_content = clean_content
    FROM DiscordMessages
    WHERE message_id = @message_id;

    -- If message doesn't exist, return error code -1
    IF @current_content IS NULL
        RETURN -1;

    -- If content has changed, return error code -2 for concurrency conflict
    IF @current_content <> @old_content OR @current_clean_content <> @old_clean_content
        RETURN -2;

    -- Insert the old message content into EditedMessages table
    INSERT INTO EditedMessages (message_id, edit_id, old_content, edit_datetime)
    SELECT 
        @message_id, 
        COALESCE(MAX(edit_id), 0) + 1,  -- Increment edit_id per message
        @old_content, 
        GETDATE()
    FROM EditedMessages
    WHERE message_id = @message_id;

    -- Update the message content in DiscordMessages table
    UPDATE DiscordMessages
    SET 
        content = @new_content,
        clean_content = @new_clean_content,
        attachment_url = @attachment_url,
        message_link = @message_link,
        isEdited = 1,  -- Mark the message as edited
        message_edited_datetime = GETDATE()
    WHERE message_id = @message_id;

    -- Return 0 to indicate success
    RETURN 0;
END;
