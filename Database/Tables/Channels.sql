CREATE TABLE Channels (
    channel_id BIGINT PRIMARY KEY,
    channel_name NVARCHAR(255) NOT NULL,
    channel_type NVARCHAR(255) NOT NULL,
    guild_id BIGINT NOT NULL,
    is_inactive BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Channel_Type FOREIGN KEY (channel_type) REFERENCES ChannelTypes(channel_type),
    CONSTRAINT FK_Channels_Guild FOREIGN KEY (guild_id) REFERENCES Guilds(guild_id)
);
