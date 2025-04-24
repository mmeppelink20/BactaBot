-- Create Procedure (Channels)
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

-- Create a table type for bulk inserting channels
CREATE TYPE ChannelsTableType AS TABLE
(
    channel_id BIGINT PRIMARY KEY,
    channel_name NVARCHAR(255) NOT NULL,
    channel_type NVARCHAR(255) NOT NULL,
    guild_id BIGINT NOT NULL
);
GO

CREATE PROCEDURE sp_insert_multiple_channels
    @Channels ChannelsTableType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    -- Ensure all channel_type values exist in ChannelTypes
    INSERT INTO ChannelTypes (channel_type)
    SELECT DISTINCT source.channel_type
    FROM @Channels AS source
    WHERE NOT EXISTS (
        SELECT 1 FROM ChannelTypes AS ct WHERE ct.channel_type = source.channel_type
    );

    -- Perform the merge operation (Insert/Update)
    MERGE INTO Channels AS target
    USING @Channels AS source
    ON target.channel_id = source.channel_id
    WHEN MATCHED AND (target.channel_name != source.channel_name OR target.channel_type != source.channel_type OR target.is_inactive = 1) THEN
        UPDATE SET 
            target.channel_name = source.channel_name,
            target.channel_type = source.channel_type,
            target.is_inactive = 0 -- Reactivate if it was previously inactive
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (channel_id, channel_name, channel_type, guild_id, is_inactive)
        VALUES (source.channel_id, source.channel_name, source.channel_type, source.guild_id, 0); -- New channels are active (not inactive)
END;
GO