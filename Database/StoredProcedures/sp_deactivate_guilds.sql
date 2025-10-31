CREATE PROCEDURE sp_deactivate_guilds
    @Guilds GuildsTable READONLY
AS
BEGIN
    SET NOCOUNT ON;

    -- Deactivate guilds that exist in the database but NOT in the input list
    UPDATE Guilds
    SET is_inactive = 1
    WHERE guild_id NOT IN (SELECT guild_id FROM @Guilds);
END;
GO