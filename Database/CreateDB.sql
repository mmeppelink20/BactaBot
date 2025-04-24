IF EXISTS (
    SELECT 1 
    FROM master.dbo.sysdatabases 
    WHERE name = 'BactaBotDB'
)
BEGIN
    DROP DATABASE BactaBotDB;
END
GO


CREATE DATABASE BactaBotDB;
GO

