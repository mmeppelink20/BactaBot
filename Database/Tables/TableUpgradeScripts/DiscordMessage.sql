ALTER TABLE DiscordMessages
ADD isDeleted BIT CONSTRAINT DF_DiscordMessages_isDeleted DEFAULT 0;
ADD message_deleted_datetime DATETIME NULL

UPDATE DiscordMessages
SET isDeleted = 0;