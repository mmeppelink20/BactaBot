-- Delete Procedure (Channels)
CREATE PROCEDURE sp_delete_channel
    @channel_id BIGINT
AS
BEGIN
    DELETE FROM Channels WHERE channel_id = @channel_id;
END;
GO