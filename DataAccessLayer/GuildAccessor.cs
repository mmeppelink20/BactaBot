using DataAccessLayerInterfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DataAccessLayer
{
    public class GuildAccessor(IDBConnection dbConnection) : IGuildAccessor
    {
        private readonly IDBConnection _dbConnection = dbConnection;
        public async Task DeactivateMultipleGuildsAsync(List<ulong> guilds)
        {
            string cmdText = "sp_deactivate_guilds";


            using (var conn = _dbConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(cmdText, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Create DataTable matching the table type
                    DataTable guildTable = new();
                    guildTable.Columns.Add("guild_id", typeof(long));
                    // Populate the DataTable
                    foreach (var guild in guilds)
                    {
                        guildTable.Rows.Add(guild);
                    }
                    // Add the table-valued parameter
                    var param = cmd.Parameters.AddWithValue("@Guilds", guildTable);
                    param.SqlDbType = SqlDbType.Structured;
                    try
                    {
                        // Execute the stored procedure
                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }

        public Task DeleteGuildAsync(ulong guildId)
        {
            throw new NotImplementedException();
        }

        public async Task InsertGuildAsync(ulong guildId, string guildName)
        {
            string cmdText = "sp_insert_guild";

            using (var conn = _dbConnection.GetConnection())
            {
                await conn.OpenAsync();

                using (var cmd = new SqlCommand(cmdText, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@guild_id", SqlDbType.BigInt).Value = guildId;
                    cmd.Parameters.Add("@guild_name", SqlDbType.VarChar).Value = guildName;

                    try
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }

        public async Task InsertMultipleGuildsAsync(Dictionary<ulong, string> guilds)
        {
            string cmdText = "sp_insert_multiple_guilds";

            using (var conn = _dbConnection.GetConnection())
            {
                await conn.OpenAsync();

                using (var cmd = new SqlCommand(cmdText, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Create DataTable matching the table type
                    DataTable guildTable = new();

                    guildTable.Columns.Add("guild_id", typeof(long));
                    guildTable.Columns.Add("guild_name", typeof(string));

                    // Populate the DataTable
                    foreach (var guild in guilds)
                    {
                        guildTable.Rows.Add(guild.Key, guild.Value);
                    }

                    // Add the table-valued parameter
                    var param = cmd.Parameters.AddWithValue("@Guilds", guildTable);
                    param.SqlDbType = SqlDbType.Structured;

                    try
                    {
                        // Execute the stored procedure
                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }

        public Task UpdateGuildAsync(ulong guildId, string guildName)
        {
            throw new NotImplementedException();
        }
    }
}
