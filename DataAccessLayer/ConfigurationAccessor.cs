using DataAccessLayerInterfaces;
using DataObjects;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DataAccessLayer
{
    public class ConfigurationAccessor(IDBConnection configuration) : IConfigurationAccessor
    {
        private readonly IDBConnection _configuration = configuration;
        
        public async Task<string> GetConfigurationKeyValueAsync(string key)
        {
            string value = string.Empty;

            var conn = _configuration.GetConnection();
            var cmd = new SqlCommand(StoredProcedure.GetConfigurationByKey, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@CONFIGURATION_KEY", SqlDbType.NVarChar, 100).Value = key;

            try
            {
                await conn.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    value = reader["CONFIGURATION_VALUE"]?.ToString() ?? "key/value pair not found";
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                await conn.CloseAsync();
            }

            return value;
        }

        public async Task<Dictionary<string, string>> GetAllConfigurationKeysValuesAsync()
        {
            Dictionary<string, string> keyValuePairs = [];

            var conn = _configuration.GetConnection();
            var cmd = new SqlCommand(StoredProcedure.GetAllConfigurationValues, conn);
            cmd.CommandType = CommandType.StoredProcedure;

            try
            {
                await conn.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    keyValuePairs.Add(
                        reader["CONFIGURATION_KEY"]?.ToString() ?? "key not found", 
                        reader["CONFIGURATION_VALUE"]?.ToString() ?? "value not found"
                    );
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                await conn.CloseAsync();
            }

            return keyValuePairs;
        }

        public async Task<bool> SetConfigurationKeyValueAsync(string key, string value)
        {
            var conn = _configuration.GetConnection();
            var cmd = new SqlCommand(StoredProcedure.SetConfigurationKeyValue, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@CONFIGURATION_KEY", SqlDbType.NVarChar, 100).Value = key;
            cmd.Parameters.Add("@CONFIGURATION_VALUE", SqlDbType.NVarChar, -1).Value = value; // -1 for MAX

            try
            {
                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public async Task<bool> DeleteConfigurationKeyAsync(string key)
        {
            var conn = _configuration.GetConnection();
            var cmd = new SqlCommand(StoredProcedure.DeleteConfigurationKey, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@CONFIGURATION_KEY", SqlDbType.NVarChar, 100).Value = key;

            try
            {
                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public async Task<bool> UpdateConfigurationKeyAsync(string key, string oldValue, string newValue)
        {
            var conn = _configuration.GetConnection();
            var cmd = new SqlCommand(StoredProcedure.UpdateConfigurationKey, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@CONFIGURATION_KEY", SqlDbType.NVarChar, 100).Value = key;
            cmd.Parameters.Add("@OLD_CONFIGURATION_VALUE", SqlDbType.NVarChar, -1).Value = oldValue;
            cmd.Parameters.Add("@NEW_CONFIGURATION_VALUE", SqlDbType.NVarChar, -1).Value = newValue;

            try
            {
                await conn.OpenAsync();
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        // Add the following methods to fully implement IConfigurationAccessor

        public async Task<ConfigurationItem?> GetConfigurationItemAsync(string key)
        {
            var conn = _configuration.GetConnection();
            var cmd = new SqlCommand(StoredProcedure.GetConfigurationByKey, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@CONFIGURATION_KEY", SqlDbType.NVarChar, 100).Value = key;

            try
            {
                await conn.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new ConfigurationItem
                    {
                        Key = reader["CONFIGURATION_KEY"]?.ToString() ?? string.Empty,
                        Value = reader["CONFIGURATION_VALUE"]?.ToString() ?? string.Empty
                        // Add other properties if ConfigurationItem has more fields
                    };
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public async Task<Dictionary<string, ConfigurationItem>> GetAllConfigurationItemsAsync()
        {
            Dictionary<string, ConfigurationItem> items = [];

            var conn = _configuration.GetConnection();
            var cmd = new SqlCommand(StoredProcedure.GetAllConfigurationValues, conn);
            cmd.CommandType = CommandType.StoredProcedure;

            try
            {
                await conn.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var key = reader["CONFIGURATION_KEY"]?.ToString() ?? string.Empty;
                    var value = reader["CONFIGURATION_VALUE"]?.ToString() ?? string.Empty;
                    items[key] = new ConfigurationItem
                    {
                        Key = key,
                        Value = value
                        // Add other properties if ConfigurationItem has more fields
                    };
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                await conn.CloseAsync();
            }

            return items;
        }

        public async Task<bool> SetConfigurationKeyValueAsync(string key, string value, bool isEncrypted = false)
        {
            // If encryption is required, add logic here. For now, just call the existing method.
            return await SetConfigurationKeyValueAsync(key, value);
        }
    }
}
