using DataAccessLayerInterfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DataAccessLayer
{
    public class ConfigurationAccessor : IConfigurationAccessor
    {
        public async Task<string> GetConfigurationKeyValueAsync(string key)
        {
            string value = string.Empty;

            var connectionFactory = new DBConnection();
            var conn = connectionFactory.GetConnection();

            var cmdText = "sp_get_configuration_by_key";

            var cmd = new SqlCommand(cmdText, conn);

            cmd.Parameters.Add("@CONFIGURATION_KEY", SqlDbType.VarChar).Value = key;

            try
            {
                await conn.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    value = reader["Value"]?.ToString() ?? "key/value pair not found";
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

            var connectionFactory = new DBConnection();
            var conn = connectionFactory.GetConnection();

            var cmdText = "sp_get_all_configuration_values";

            var cmd = new SqlCommand(cmdText, conn);

            try
            {
                await conn.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    keyValuePairs.Add(reader["configuration_key"]?.ToString() ?? "key not found", reader["configuration_value"]?.ToString() ?? "value not found");
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
    }

}
