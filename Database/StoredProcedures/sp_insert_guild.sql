CREATE PROCEDURE sp_insert_guild
    @guild_id BIGINT,
    @guild_name NVARCHAR(255)
AS
BEGIN
    INSERT INTO Guilds (guild_id, guild_name, is_inactive)
    VALUES (@guild_id, @guild_name, 0); -- Set to 0 (not inactive)
END;
GO
