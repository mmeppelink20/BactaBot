CREATE TYPE ChannelsTableType AS TABLE
(
    channel_id BIGINT NOT NULL PRIMARY KEY,
    channel_name NVARCHAR(255) NOT NULL,
    channel_type NVARCHAR(255) NOT NULL,
    guild_id BIGINT NOT NULL
);
GO