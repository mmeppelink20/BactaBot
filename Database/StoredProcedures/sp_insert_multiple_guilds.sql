CREATE PROCEDURE sp_insert_multiple_guilds
    @Guilds GuildsTableType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    -- Perform the merge operation (Insert/Update)
    MERGE INTO Guilds AS target
    USING @Guilds AS source
    ON target.guild_id = source.guild_id
    WHEN MATCHED AND (target.guild_name != source.guild_name OR target.is_inactive = 1) THEN
        UPDATE SET 
            target.guild_name = source.guild_name,
            target.is_inactive = 0 -- Reactivate if previously inactive
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (guild_id, guild_name, is_inactive)
        VALUES (source.guild_id, source.guild_name, 0); -- New guilds are active (not inactive)
END;
GO