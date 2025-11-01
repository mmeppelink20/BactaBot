-- For JOIN performance in message queries
CREATE NONCLUSTERED INDEX IX_Users_Username 
ON Users (username)
INCLUDE (is_bot, avatar_url);