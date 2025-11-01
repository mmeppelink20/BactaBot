-- Check if database exists and handle active connections
IF EXISTS (
    SELECT 1 
    FROM master.dbo.sysdatabases 
    WHERE name = 'BactaBotDB'
)
BEGIN
    PRINT 'Database BactaBotDB exists. Attempting to drop...'
    
    -- Set database to single user mode to kill all connections
    ALTER DATABASE BactaBotDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    
    -- Now drop the database
    DROP DATABASE BactaBotDB;
    
    PRINT 'Database BactaBotDB dropped successfully.'
END
ELSE
BEGIN
    PRINT 'Database BactaBotDB does not exist.'
END
GO

PRINT 'Creating database BactaBotDB...'
CREATE DATABASE BactaBotDB;
PRINT 'Database BactaBotDB created successfully.'
GO