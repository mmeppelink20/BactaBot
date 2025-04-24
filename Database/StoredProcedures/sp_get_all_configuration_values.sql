CREATE PROCEDURE [dbo].[sp_get_all_configuration_values]
AS
BEGIN
    SELECT [CONFIGURATION_KEY], [CONFIGURATION_VALUE]
    FROM [Configuration];
END;
