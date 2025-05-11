using DataAccessLayerInterfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DataAccessLayer
{
    public class ChannelAccessor(IDBConnection dbConnection) : IChannelAccessor
    {
        private readonly IDBConnection _dbConnection = dbConnection;
        public async Task DeactivateMultipleChannelsAsync(List<ulong> channels)
        {
            string cmdText = "sp_deactivate_channels";

            using (var conn = _dbConnection.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(cmdText, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    DataTable channelTable = new();
                    channelTable.Columns.Add("channel_id", typeof(long));
                    foreach (var channel in channels)
                    {
                        channelTable.Rows.Add(channel);
                    }
                    cmd.Parameters.Add("@Channels", SqlDbType.Structured).Value = channelTable;
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

        public Task DeleteChannelAsync(ulong channelId)
        {
            throw new NotImplementedException();
        }

        public Task InsertChannelAsync(ulong channelId, string channelName)
        {
            throw new NotImplementedException();
        }

        public async Task InsertMultipleChannelsAsync(List<(ulong guildId, ulong channelId, string channelName, string channelType)> channels)
        {
            string cmdText = "sp_insert_multiple_channels";

            using (var conn = _dbConnection.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(cmdText, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    DataTable channelTable = new();

                    channelTable.Columns.Add("channel_id", typeof(long));
                    channelTable.Columns.Add("channel_name", typeof(string));
                    channelTable.Columns.Add("channel_type", typeof(string));
                    channelTable.Columns.Add("guild_id", typeof(long));

                    foreach (var (guildId, channelId, channelName, channelType) in channels)
                    {
                        channelTable.Rows.Add(channelId, channelName, channelType, guildId);
                    }
                    cmd.Parameters.Add("@Channels", SqlDbType.Structured).Value = channelTable;
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

        public Task UpdateChannelAsync(ulong channelId, string channelName)
        {
            throw new NotImplementedException();
        }
    }
}
