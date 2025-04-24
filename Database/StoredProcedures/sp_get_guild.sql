-- Read Procedure (Single Guild)
CREATE PROCEDURE sp_get_guild
    @guild_id BIGINT
AS
BEGIN
    SELECT * FROM Guilds WHERE guild_id = @guild_id;
END;
GO

-- Read Procedure (All Guilds)
CREATE PROCEDURE sp_get_all_guilds
AS
BEGIN
    SELECT * FROM Guilds;
END;
GO