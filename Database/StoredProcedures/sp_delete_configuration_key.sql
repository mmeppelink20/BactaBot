CREATE PROCEDURE [dbo].[sp_delete_configuration_key]
    @CONFIGURATION_KEY NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM Configuration 
    WHERE configuration_key = @CONFIGURATION_KEY;

    -- Return the number of rows affected
    SELECT @@ROWCOUNT AS RowsAffected;
END;