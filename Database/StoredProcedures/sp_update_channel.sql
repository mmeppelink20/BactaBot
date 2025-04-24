-- Update Procedure (Channels)
CREATE PROCEDURE sp_update_channel
    @channel_id BIGINT,
    @old_channel_name NVARCHAR(255),
    @new_channel_name NVARCHAR(255)
AS
BEGIN
    UPDATE Channels
    SET channel_name = @new_channel_name
    WHERE channel_id = @channel_id 
          AND channel_name = @old_channel_name 

    RETURN @@ROWCOUNT;
END;
GO