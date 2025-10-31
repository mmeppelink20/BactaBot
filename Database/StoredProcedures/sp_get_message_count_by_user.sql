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