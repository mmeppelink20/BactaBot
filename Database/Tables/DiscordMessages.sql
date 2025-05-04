CREATE TABLE DiscordMessages (
    message_id BIGINT PRIMARY KEY,
    channel_id BIGINT NOT NULL,
    user_id BIGINT NOT NULL,
    content NVARCHAR(2000) NOT NULL,
    clean_content NVARCHAR(2000) NOT NULL,
    message_datetime DATETIME NOT NULL,
    isEdited BIT,
    message_edited_datetime DATETIME,
    attachment_url NVARCHAR(MAX),
    message_link NVARCHAR(MAX),
    replied_to_message_id BIGINT DEFAULT NULL,
    isDeleted BIT,
    message_deleted_datetime DATETIME NULL,
    CONSTRAINT FK_DiscordMessage_Channel FOREIGN KEY (channel_id) REFERENCES Channels(channel_id),
    CONSTRAINT FK_DiscordMessage_Author FOREIGN KEY (user_id) REFERENCES Users(user_id)
);

CREATE TABLE EditedMessages (
    message_id BIGINT NOT NULL,
    edit_id INT NOT NULL,  -- Starts at 1 per message_id, increments with each edit
    old_content NVARCHAR(2000) NOT NULL,
    edit_datetime DATETIME NOT NULL,
    CONSTRAINT PK_EditedMessages PRIMARY KEY (message_id, edit_id),
    CONSTRAINT FK_EditedMessage_Original FOREIGN KEY (message_id) REFERENCES DiscordMessages(message_id)
);