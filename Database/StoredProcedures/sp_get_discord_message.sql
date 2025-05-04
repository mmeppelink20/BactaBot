-- Read (Select)
CREATE PROCEDURE sp_get_discord_message
    @message_id BIGINT
AS
BEGIN
    SELECT * FROM DiscordMessages WHERE message_id = @message_id;
END;
GO

CREATE PROCEDURE sp_get_message_count_by_user
    @search_content NVARCHAR(2000),
    @user_id BIGINT = NULL,
    @channel_id BIGINT = NULL,
    @guild_id BIGINT = NULL
AS
BEGIN
    SELECT 
        dm.user_id,
        u.username,
        COUNT(*) AS message_count
    FROM DiscordMessages dm
    JOIN Users u ON dm.user_id = u.user_id
    JOIN Channels c ON dm.channel_id = c.channel_id
    WHERE dm.content LIKE '%' + @search_content + '%'
      AND (@user_id IS NULL OR dm.user_id = @user_id)
      AND (@channel_id IS NULL OR dm.channel_id = @channel_id)
      AND (@guild_id IS NULL OR c.guild_id = @guild_id)
    GROUP BY dm.user_id, u.username
    ORDER BY message_count DESC;
END;
GO


CREATE PROCEDURE sp_get_recent_discord_messages
    @minutes_back INT = 60,
    @channel_id BIGINT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        dm.message_id,
        dm.channel_id,
        dm.user_id,
        u.username,
        gu.nickname,
        dm.content,
        dm.clean_content,
        dm.message_datetime,
        dm.isEdited,
        dm.message_edited_datetime,
        dm.attachment_url,
        dm.message_link,
        dm.replied_to_message_id,
        dm.isDeleted,
        dm.message_deleted_datetime,
    FROM DiscordMessages dm
    INNER JOIN Users u ON dm.user_id = u.user_id
    INNER JOIN Channels c ON dm.channel_id = c.channel_id
    LEFT JOIN GuildUsers gu ON gu.user_id = dm.user_id AND gu.guild_id = c.guild_id
    WHERE dm.message_datetime >= DATEADD(MINUTE, -@minutes_back, GETDATE())
      AND (@channel_id IS NULL OR dm.channel_id = @channel_id)
    ORDER BY dm.message_datetime DESC;
END;