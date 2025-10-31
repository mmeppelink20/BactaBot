CREATE TABLE EditedMessages (
    message_id BIGINT NOT NULL,
    edit_id INT NOT NULL,  -- Starts at 1 per message_id, increments with each edit
    old_content NVARCHAR(2000) NOT NULL,
    edit_datetime DATETIME NOT NULL,
    CONSTRAINT PK_EditedMessages PRIMARY KEY (message_id, edit_id),
    CONSTRAINT FK_EditedMessage_Original FOREIGN KEY (message_id) REFERENCES DiscordMessages(message_id)
);