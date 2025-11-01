CREATE PROCEDURE [dbo].[sp_update_configuration_key]
    @CONFIGURATION_KEY NVARCHAR(100),
    @OLD_CONFIGURATION_VALUE NVARCHAR(MAX),
    @NEW_CONFIGURATION_VALUE NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    -- Update only if the current value matches the expected old value (concurrency check)
    UPDATE Configuration
    SET configuration_value = @NEW_CONFIGURATION_VALUE
    WHERE configuration_key = @CONFIGURATION_KEY 
          AND configuration_value = @OLD_CONFIGURATION_VALUE;

    -- Return the number of rows affected
    SELECT @@ROWCOUNT AS RowsAffected;
END;