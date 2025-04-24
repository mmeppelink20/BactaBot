CREATE TYPE UsersIDs AS TABLE
(
    user_id BIGINT,
    guild_id BIGINT,
    PRIMARY KEY (user_id, guild_id) -- Composite primary key
);
GO

CREATE PROCEDURE sp_deactivate_guild_users
    @Users UsersIDs READONLY
AS
BEGIN
    SET NOCOUNT ON;

    -- Deactivate users in the GuildUsers table that are not in the input UsersIDs list
    UPDATE GuildUsers
    SET is_inactive = 1
    WHERE NOT EXISTS (
        SELECT 1 
        FROM @Users u 
        WHERE u.user_id = GuildUsers.user_id 
          AND u.guild_id = GuildUsers.guild_id
    );
END;
GO

    
