-- Description: This script contains the stored procedure for inserting a single guild into the Guilds table.

-- Create the stored procedure for inserting a single guild
CREATE PROCEDURE sp_insert_guild
    @guild_id BIGINT,
    @guild_name NVARCHAR(255)
AS
BEGIN
    INSERT INTO Guilds (guild_id, guild_name, is_inactive)
    VALUES (@guild_id, @guild_name, 0); -- Set to 0 (not inactive)
END;
GO

-- Create a table type for bulk inserting guilds
CREATE TYPE GuildsTableType AS TABLE
(
    guild_id BIGINT,
    guild_name NVARCHAR(255)
);
GO

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
