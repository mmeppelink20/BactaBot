using DataAccessLayerInterfaces;
using DataObjects;
using Discord;
using Discord.WebSocket;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DataAccessLayer
{
    public class DiscordMessageAccessor(IDBConnection dbConnection) : IDiscordMessageAccessor
    {
        public async Task<bool> DeleteDiscordMessage(ulong messageID)
        {
            const string cmdText = "sp_delete_discord_message";
            using (var conn = dbConnection.GetConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                using (var cmd = new SqlCommand(cmdText, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    var parameters = new (string, SqlDbType, object)[]
                    {
                        ("@message_id", SqlDbType.BigInt, messageID),
                        ("@message_deleted_datetime", SqlDbType.DateTime, DateTime.UtcNow)
                    };

                    foreach (var (name, type, value) in parameters)
                    {
                        cmd.Parameters.Add(name, type).Value = value;
                    }

                    var rowsAffected = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    return rowsAffected > 0;
                }
            }
        }

        public Task<SocketMessage> GetDiscordMessage(DiscordMessage message)
        {
            throw new NotImplementedException();
        }

        public async Task<List<DiscordMessageVM>> GetDiscordMessagesByChannelIDAndMinutesAsync(ulong channelID, int minutes)
        {
            const string cmdText = "sp_get_recent_discord_messages";

            using (var conn = dbConnection.GetConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);

                using (var cmd = new SqlCommand(cmdText, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@channel_id", (long)channelID);
                    cmd.Parameters.AddWithValue("@minutes_back", minutes);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        var messages = new List<DiscordMessageVM>();

                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            var message = new DiscordMessageVM
                            {
                                MessageId = Convert.ToUInt64(reader.GetInt64(0)),
                                ChannelId = Convert.ToUInt64(reader.GetInt64(1)),
                                UserId = Convert.ToUInt64(reader.GetInt64(2)),
                                UserName = reader.IsDBNull(3) ? null : reader.GetString(3),
                                NickName = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Content = reader.IsDBNull(5) ? null : reader.GetString(5),
                                CleanContent = reader.IsDBNull(6) ? null : reader.GetString(6),
                                MessageDatetime = reader.GetDateTime(7),
                                IsEdited = reader.GetBoolean(8),
                                MessageEditedDatetime = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                                AttachmentUrl = reader.IsDBNull(10) ? null : reader.GetString(10),
                                MessageLink = reader.IsDBNull(11) ? null : reader.GetString(11),
                                RepliedToMessageId = reader.IsDBNull(12) ? null : Convert.ToUInt64(reader.GetInt64(12)),
                                IsDeleted = reader.GetBoolean(13),
                                MessageDeletedDatetime = reader.IsDBNull(14) ? null : reader.GetDateTime(14)
                            };

                            messages.Add(message);
                        }

                        return messages;
                    }
                }
            }
        }

        public Task<List<SocketMessage>> GetDiscordMessagesByUserChannel(ulong userId, ulong channelId, ulong guildId)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, int>> GetMessageCountByUser(string message)
        {
            throw new NotImplementedException();
        }

        public async Task InsertDiscordMessage(DiscordMessageVM message)
        {
            const string cmdText = "sp_insert_discord_message";

            using (var conn = dbConnection.GetConnection())
            {
                await conn.OpenAsync();

                using (var cmd = new SqlCommand(cmdText, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    var parameters = new (string, SqlDbType, object)[]
                    {
                        ("@message_id", SqlDbType.BigInt, message.MessageId),
                        ("@channel_id", SqlDbType.BigInt, message.ChannelId),
                        ("@user_id", SqlDbType.BigInt, message.UserId),
                        ("@user_name", SqlDbType.NVarChar, message.UserName ?? (object)DBNull.Value),
                        ("@nickname", SqlDbType.NVarChar, message.NickName ?? (object)DBNull.Value),
                        ("@joined_at", SqlDbType.DateTime, message.MessageDatetime),
                        ("@is_inactive", SqlDbType.Bit, false),
                        ("@is_bot", SqlDbType.Bit, false),
                        ("@avatar_url", SqlDbType.NVarChar, message.AvatarUrl ?? (object)DBNull.Value),
                        ("@content", SqlDbType.NVarChar, message.Content ?? (object)DBNull.Value),
                        ("@clean_content", SqlDbType.NVarChar, message.CleanContent ?? (object)DBNull.Value),
                        ("@message_datetime", SqlDbType.DateTime, message.MessageDatetime),
                        ("@isEdited", SqlDbType.Bit, message.IsEdited),
                        ("@message_edited_datetime", SqlDbType.DateTime, message.MessageEditedDatetime ?? (object)DBNull.Value),
                        ("@attachment_url", SqlDbType.NVarChar, message.AttachmentUrl ?? (object)DBNull.Value),
                        ("@message_link", SqlDbType.NVarChar, message.MessageLink ?? (object)DBNull.Value),
                        ("@replied_to_message_id", SqlDbType.BigInt, message.RepliedToMessageId ?? (object)DBNull.Value),
                        ("@channel_name", SqlDbType.NVarChar, message.ChannelName ?? (object)DBNull.Value),
                        ("@channel_type", SqlDbType.NVarChar, message.ChannelType ?? (object)DBNull.Value),
                        ("@guild_id", SqlDbType.BigInt, message.GuildId ?? (object)DBNull.Value)
                    };

                    foreach (var (name, type, value) in parameters)
                    {
                        cmd.Parameters.Add(name, type).Value = value;
                    }

                    await cmd.ExecuteNonQueryAsync(); // No try-catch, letting it bubble up
                }
            }
        }
        public async Task InsertDiscordMessages(IEnumerable<DiscordMessageVM> messages)
        {
            const string cmdText = "sp_bulk_insert_discord_messages";

            var dataTable = new DataTable();
            dataTable.Columns.Add("message_id", typeof(long));
            dataTable.Columns.Add("channel_id", typeof(long));
            dataTable.Columns.Add("user_id", typeof(long));
            dataTable.Columns.Add("user_name", typeof(string));
            dataTable.Columns.Add("nickname", typeof(string));
            dataTable.Columns.Add("joined_at", typeof(DateTime));
            dataTable.Columns.Add("is_inactive", typeof(bool));
            dataTable.Columns.Add("is_bot", typeof(bool));
            dataTable.Columns.Add("avatar_url", typeof(string));
            dataTable.Columns.Add("content", typeof(string));
            dataTable.Columns.Add("clean_content", typeof(string));
            dataTable.Columns.Add("message_datetime", typeof(DateTime));
            dataTable.Columns.Add("isEdited", typeof(bool));
            dataTable.Columns.Add("message_edited_datetime", typeof(DateTime));
            dataTable.Columns.Add("attachment_url", typeof(string));
            dataTable.Columns.Add("message_link", typeof(string));
            dataTable.Columns.Add("replied_to_message_id", typeof(long));
            dataTable.Columns.Add("channel_name", typeof(string));
            dataTable.Columns.Add("channel_type", typeof(string));
            dataTable.Columns.Add("guild_id", typeof(long));

            foreach (var m in messages)
            {
                dataTable.Rows.Add(
                    (long)m.MessageId,
                    (long)m.ChannelId,
                    (long)m.UserId,
                    m.UserName ?? (object)DBNull.Value,
                    m.NickName ?? (object)DBNull.Value,
                    m.JoinedDate ?? (object)DBNull.Value,
                    false, // is_inactive (set as needed)  
                    false, // is_bot (set as needed)  
                    m.AvatarUrl ?? (object)DBNull.Value,
                    m.Content ?? (object)DBNull.Value,
                    m.CleanContent ?? (object)DBNull.Value,
                    m.MessageDatetime,
                    m.IsEdited,
                    m.MessageEditedDatetime ?? (object)DBNull.Value,
                    m.AttachmentUrl ?? (object)DBNull.Value,
                    m.MessageLink ?? (object)DBNull.Value,
                    m.RepliedToMessageId ?? (object)DBNull.Value,
                    m.ChannelName ?? (object)DBNull.Value,
                    m.ChannelType ?? (object)DBNull.Value,
                    m.GuildId ?? (object)DBNull.Value
                );
            }

            using (var conn = dbConnection.GetConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);

                using (var cmd = new SqlCommand(cmdText, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    var param = cmd.Parameters.AddWithValue("@messages", dataTable);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.DiscordMessageType";

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public Task UpdateDiscordMessage(DiscordMessage message)
        {
            throw new NotImplementedException();
        }

    }
}
