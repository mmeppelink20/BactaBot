CREATE TYPE dbo.DiscordMessageType AS TABLE (
    message_id BIGINT,
    channel_id BIGINT,
    user_id BIGINT,
    user_name NVARCHAR(255),
    nickname NVARCHAR(255),
    joined_at DATETIME,
    is_inactive BIT,
    is_bot BIT,
    avatar_url NVARCHAR(255),
    content NVARCHAR(4000),
    clean_content NVARCHAR(4000),
    message_datetime DATETIME,
    isEdited BIT,
    message_edited_datetime DATETIME,
    attachment_url NVARCHAR(MAX),
    message_link NVARCHAR(MAX),
    replied_to_message_id BIGINT,
    channel_name NVARCHAR(255),
    channel_type NVARCHAR(255),
    guild_id BIGINT
);
GO