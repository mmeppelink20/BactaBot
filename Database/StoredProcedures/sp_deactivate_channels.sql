-- Create a table type for bulk deactivating channels
CREATE TYPE ChannelsIDs AS TABLE
(
    channel_id BIGINT PRIMARY KEY
);
GO

CREATE PROCEDURE sp_deactivate_channels
    @Channels ChannelsIDs READONLY
AS
BEGIN
    SET NOCOUNT ON;

    -- Deactivate channels that exist in the database but NOT in the input list
    UPDATE Channels
    SET is_inactive = 1
    WHERE channel_id NOT IN (SELECT channel_id FROM @Channels);
END;
GO