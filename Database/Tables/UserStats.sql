CREATE TABLE UserStats (
    user_id BIGINT,
    guild_id BIGINT,
    user_level INT DEFAULT 1,
    experience BIGINT DEFAULT 0,
    currency_amount BIGINT DEFAULT 0,
    PRIMARY KEY (user_id, guild_id),
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (guild_id) REFERENCES Guilds(guild_id) ON DELETE CASCADE
);
