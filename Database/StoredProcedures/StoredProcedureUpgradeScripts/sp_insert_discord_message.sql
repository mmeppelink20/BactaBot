ALTER PROCEDURE sp_insert_discord_message
    @message_id BIGINT,
    @channel_id BIGINT,
    @user_id BIGINT,
    @user_name NVARCHAR(255),
    @nickname NVARCHAR(255),
    @joined_at DATETIME,
    @is_inactive BIT,
    @is_bot BIT,
    @avatar_url NVARCHAR(255),
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

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Upsert ChannelType
        MERGE ChannelTypes AS target
        USING (SELECT @channel_type AS channel_type) AS source
        ON target.channel_type = source.channel_type
        WHEN NOT MATCHED THEN
            INSERT (channel_type) VALUES (source.channel_type);

        -- Upsert User
        MERGE Users AS target
        USING (SELECT @user_id AS user_id) AS source
        ON target.user_id = source.user_id
        WHEN MATCHED THEN
            UPDATE SET username = @user_name, is_bot = @is_bot, avatar_url = @avatar_url
        WHEN NOT MATCHED THEN
            INSERT (user_id, username, is_bot, avatar_url)
            VALUES (@user_id, @user_name, @is_bot, @avatar_url);

        -- Upsert GuildUser
        MERGE GuildUsers AS target
        USING (SELECT @guild_id AS guild_id, @user_id AS user_id) AS source
        ON target.guild_id = source.guild_id AND target.user_id = source.user_id
        WHEN MATCHED AND 
            (
                (target.nickname IS NULL AND @nickname IS NOT NULL) OR
                (target.nickname IS NOT NULL AND @nickname IS NULL) OR
                (target.nickname IS NOT NULL AND @nickname IS NOT NULL AND target.nickname <> @nickname)
            ) THEN
            UPDATE SET nickname = @nickname
        WHEN NOT MATCHED THEN
            INSERT (guild_id, user_id, nickname, joined_at, is_inactive)
            VALUES (@guild_id, @user_id, @nickname, @joined_at, @is_inactive);

        -- Upsert Channel
        MERGE Channels AS target
        USING (SELECT @channel_id AS channel_id) AS source
        ON target.channel_id = source.channel_id
        WHEN NOT MATCHED THEN
            INSERT (channel_id, channel_name, channel_type, guild_id, is_inactive)
            VALUES (@channel_id, @channel_name, @channel_type, @guild_id, 0);

        -- Upsert Message with content comparison
        MERGE DiscordMessages AS target
        USING (SELECT @message_id AS message_id) AS source
        ON target.message_id = source.message_id
        WHEN MATCHED AND 
            (
                -- Only update if content has actually changed
                (target.content IS NULL AND @content IS NOT NULL) OR
                (target.content IS NOT NULL AND @content IS NULL) OR
                (target.content IS NOT NULL AND @content IS NOT NULL AND target.content <> @content) OR
                (target.clean_content IS NULL AND @clean_content IS NOT NULL) OR
                (target.clean_content IS NOT NULL AND @clean_content IS NULL) OR
                (target.clean_content IS NOT NULL AND @clean_content IS NOT NULL AND target.clean_content <> @clean_content) OR
                -- Also check other fields that might have changed
                (target.attachment_url IS NULL AND @attachment_url IS NOT NULL) OR
                (target.attachment_url IS NOT NULL AND @attachment_url IS NULL) OR
                (target.attachment_url IS NOT NULL AND @attachment_url IS NOT NULL AND target.attachment_url <> @attachment_url) OR
                (target.message_link IS NULL AND @message_link IS NOT NULL) OR
                (target.message_link IS NOT NULL AND @message_link IS NULL) OR
                (target.message_link IS NOT NULL AND @message_link IS NOT NULL AND target.message_link <> @message_link) OR
                (target.replied_to_message_id IS NULL AND @replied_to_message_id IS NOT NULL) OR
                (target.replied_to_message_id IS NOT NULL AND @replied_to_message_id IS NULL) OR
                (target.replied_to_message_id IS NOT NULL AND @replied_to_message_id IS NOT NULL AND target.replied_to_message_id <> @replied_to_message_id)
            ) THEN
            UPDATE SET 
                content = @content,
                clean_content = @clean_content,
                isEdited = @isEdited,
                message_edited_datetime = @message_edited_datetime,
                attachment_url = @attachment_url,
                message_link = @message_link,
                replied_to_message_id = @replied_to_message_id
        WHEN NOT MATCHED THEN
            INSERT (
                message_id, channel_id, user_id, content, clean_content,
                message_datetime, isEdited, message_edited_datetime,
                attachment_url, message_link, replied_to_message_id
            )
            VALUES (
                @message_id, @channel_id, @user_id, @content, @clean_content,
                @message_datetime, @isEdited, @message_edited_datetime,
                @attachment_url, @message_link, @replied_to_message_id
            );

        COMMIT TRANSACTION;
        RETURN 0; -- Success
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @error_message NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR('Error in sp_insert_discord_message: %s', 16, 1, @error_message);
        RETURN -1;
    END CATCH
END;
GO