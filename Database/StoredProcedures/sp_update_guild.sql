-- Update Procedure
CREATE PROCEDURE sp_update_guild
    @guild_id BIGINT,
    @old_guild_name NVARCHAR(255),
    @new_guild_name NVARCHAR(255)
AS
BEGIN
    UPDATE Guilds
    SET guild_name = @new_guild_name
    WHERE guild_id = @guild_id AND guild_name = @old_guild_name;

    RETURN @@ROWCOUNT;
END;
GO