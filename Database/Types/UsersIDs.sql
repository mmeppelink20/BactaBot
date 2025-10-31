CREATE TYPE UsersIDs AS TABLE
(
    user_id BIGINT,
    guild_id BIGINT,
    PRIMARY KEY (user_id, guild_id) -- Composite primary key
);
GO