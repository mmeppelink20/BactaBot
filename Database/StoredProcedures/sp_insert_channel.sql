CREATE PROCEDURE sp_insert_channel
    @channel_id BIGINT,
    @channel_name NVARCHAR(255),
    @channel_type NVARCHAR(255),
    @guild_id BIGINT
AS
BEGIN
    INSERT INTO Channels (channel_id, channel_name, channel_type, guild_id, is_inactive)
    VALUES (@channel_id, @channel_name, @channel_type, @guild_id, 0);
END;
GO