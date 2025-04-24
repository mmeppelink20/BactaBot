-- Read Procedure (Single Channel)
CREATE PROCEDURE sp_get_channel
    @channel_id BIGINT
AS
BEGIN
    SELECT * FROM Channels WHERE channel_id = @channel_id;
END;
GO

-- Read Procedure (All Channels)
CREATE PROCEDURE sp_get_all_channels
AS
BEGIN
    SELECT * FROM Channels;
END;
GO