-- Delete Procedure with Concurrency Check
CREATE PROCEDURE sp_delete_guild
    @guild_id BIGINT,
    @guild_name NVARCHAR(255)
AS
BEGIN
    DELETE FROM Guilds WHERE guild_id = @guild_id AND guild_name = @guild_name;

    RETURN @@ROWCOUNT;
END;
GO