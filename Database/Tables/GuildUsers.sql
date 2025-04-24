CREATE TABLE GuildUsers (
    guild_id BIGINT,
    user_id BIGINT,
    nickname NVARCHAR(255) NULL,
    joined_at DATETIME NOT NULL DEFAULT GETDATE(),
    is_inactive BIT NOT NULL DEFAULT 0,
    PRIMARY KEY (guild_id, user_id),
    FOREIGN KEY (guild_id) REFERENCES Guilds (guild_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES Users (user_id) ON DELETE CASCADE
);
    