CREATE PROCEDURE sp_get_guild
    @guild_id BIGINT
AS
BEGIN
    SELECT * FROM Guilds WHERE guild_id = @guild_id;
END;
GO