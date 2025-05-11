USE BactaBotDB

ALTER TABLE DiscordMessages
ADD isDeleted BIT CONSTRAINT DF_DiscordMessages_isDeleted DEFAULT 0;
GO

ALTER TABLE DiscordMessages
ADD message_deleted_datetime DATETIME NULL;
GO

UPDATE DiscordMessages
SET isDeleted = 0;
