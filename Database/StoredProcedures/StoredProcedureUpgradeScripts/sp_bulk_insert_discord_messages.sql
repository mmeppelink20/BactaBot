-- Alter the existing sp_bulk_insert_discord_messages procedure with content comparison
-- This ensures messages are only updated when content actually changes

ALTER PROCEDURE sp_bulk_insert_discord_messages
    @messages DiscordMessageType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Upsert ChannelTypes
        MERGE ChannelTypes AS target
        USING (SELECT DISTINCT channel_type FROM @messages WHERE channel_type IS NOT NULL) AS source
        ON target.channel_type = source.channel_type
        WHEN NOT MATCHED THEN
            INSERT (channel_type) VALUES (source.channel_type);

        -- Upsert Users
        MERGE Users AS target
        USING (SELECT DISTINCT user_id, user_name, is_bot, avatar_url FROM @messages) AS source
        ON target.user_id = source.user_id
        WHEN MATCHED THEN
            UPDATE SET username = source.user_name, is_bot = source.is_bot, avatar_url = source.avatar_url
        WHEN NOT MATCHED THEN
            INSERT (user_id, username, is_bot, avatar_url)
            VALUES (source.user_id, source.user_name, source.is_bot, source.avatar_url);

        -- Upsert GuildUsers
        MERGE GuildUsers AS target
        USING (SELECT DISTINCT guild_id, user_id, nickname, joined_at, is_inactive FROM @messages WHERE guild_id IS NOT NULL) AS source
        ON target.guild_id = source.guild_id AND target.user_id = source.user_id
        WHEN MATCHED AND 
            (
                (target.nickname IS NULL AND source.nickname IS NOT NULL) OR
                (target.nickname IS NOT NULL AND source.nickname IS NULL) OR
                (target.nickname IS NOT NULL AND source.nickname IS NOT NULL AND target.nickname <> source.nickname)
            ) THEN
            UPDATE SET nickname = source.nickname
        WHEN NOT MATCHED THEN
            INSERT (guild_id, user_id, nickname, joined_at, is_inactive)
            VALUES (source.guild_id, source.user_id, source.nickname, source.joined_at, source.is_inactive);

        -- Upsert Channels
        MERGE Channels AS target
        USING (SELECT DISTINCT channel_id, channel_name, channel_type, guild_id FROM @messages WHERE channel_id IS NOT NULL) AS source
        ON target.channel_id = source.channel_id
        WHEN NOT MATCHED THEN
            INSERT (channel_id, channel_name, channel_type, guild_id, is_inactive)
            VALUES (source.channel_id, source.channel_name, source.channel_type, source.guild_id, 0);

        -- Upsert Messages with content comparison
        MERGE DiscordMessages AS target
        USING @messages AS source
        ON target.message_id = source.message_id
        WHEN MATCHED AND 
            (
                -- Only update if content has actually changed
                (target.content IS NULL AND source.content IS NOT NULL) OR
                (target.content IS NOT NULL AND source.content IS NULL) OR
                (target.content IS NOT NULL AND source.content IS NOT NULL AND target.content <> source.content) OR
                (target.clean_content IS NULL AND source.clean_content IS NOT NULL) OR
                (target.clean_content IS NOT NULL AND source.clean_content IS NULL) OR
                (target.clean_content IS NOT NULL AND source.clean_content IS NOT NULL AND target.clean_content <> source.clean_content) OR
                -- Also check other fields that might have changed
                (target.attachment_url IS NULL AND source.attachment_url IS NOT NULL) OR
                (target.attachment_url IS NOT NULL AND source.attachment_url IS NULL) OR
                (target.attachment_url IS NOT NULL AND source.attachment_url IS NOT NULL AND target.attachment_url <> source.attachment_url) OR
                (target.message_link IS NULL AND source.message_link IS NOT NULL) OR
                (target.message_link IS NOT NULL AND source.message_link IS NULL) OR
                (target.message_link IS NOT NULL AND source.message_link IS NOT NULL AND target.message_link <> source.message_link) OR
                (target.replied_to_message_id IS NULL AND source.replied_to_message_id IS NOT NULL) OR
                (target.replied_to_message_id IS NOT NULL AND source.replied_to_message_id IS NULL) OR
                (target.replied_to_message_id IS NOT NULL AND source.replied_to_message_id IS NOT NULL AND target.replied_to_message_id <> source.replied_to_message_id)
            ) THEN
            UPDATE SET 
                content = source.content,
                clean_content = source.clean_content,
                isEdited = source.isEdited,
                message_edited_datetime = source.message_edited_datetime,
                attachment_url = source.attachment_url,
                message_link = source.message_link,
                replied_to_message_id = source.replied_to_message_id
        WHEN NOT MATCHED THEN
            INSERT (
                message_id, channel_id, user_id, content, clean_content,
                message_datetime, isEdited, message_edited_datetime,
                attachment_url, message_link, replied_to_message_id
            )
            VALUES (
                source.message_id, source.channel_id, source.user_id, source.content, source.clean_content,
                source.message_datetime, source.isEdited, source.message_edited_datetime,
                source.attachment_url, source.message_link, source.replied_to_message_id
            );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @error_message NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR('Error in sp_bulk_insert_discord_messages: %s', 16, 1, @error_message);
        THROW;
    END CATCH
END;
GO