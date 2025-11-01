-- For configuration key lookups (this table is queried frequently)
CREATE UNIQUE NONCLUSTERED INDEX IX_Configuration_Key 
ON Configuration (configuration_key)
INCLUDE (configuration_value);