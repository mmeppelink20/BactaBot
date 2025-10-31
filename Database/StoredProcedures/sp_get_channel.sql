CREATE PROCEDURE sp_get_channel
    @channel_id BIGINT
AS
BEGIN
    SELECT * FROM Channels WHERE channel_id = @channel_id;
END;
GO