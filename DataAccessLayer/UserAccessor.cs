using DataAccessLayerInterfaces;
using DataObjects;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DataAccessLayer
{
    public class UserAccessor(IDBConnection dbConnection) : IUserAccessor
    {
        private readonly IDBConnection _dbConnection = dbConnection;

        public async Task DeactivateMultipleUsersAsync(Dictionary<ulong, User> users)
        {
            using var conn = _dbConnection.GetConnection();
            await conn.OpenAsync();

            using var cmd = new SqlCommand(StoredProcedure.DeactivateGuildUsers, conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@Channels", SqlDbType.Structured).Value = CreateUserIdTable(users);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task InsertMultipleUsersAsync(Dictionary<ulong, User> users)
        {
            using var conn = _dbConnection.GetConnection();
            await conn.OpenAsync();

            using var cmd = new SqlCommand(StoredProcedure.InsertMultipleUsers, conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@Users", SqlDbType.Structured).Value = CreateUserDataTable(users);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task InsertGuildUsers(Dictionary<ulong, List<GuildUser>> users)
        {
            using var conn = _dbConnection.GetConnection();
            await conn.OpenAsync();

            using var cmd = new SqlCommand(StoredProcedure.InsertGuildUsers, conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@GuildUsers", SqlDbType.Structured).Value = CreateGuildUserDataTable(users);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeactivateGuildUsers(Dictionary<ulong, List<GuildUser>> users)
        {
            using var conn = _dbConnection.GetConnection();
            await conn.OpenAsync();

            using var cmd = new SqlCommand(StoredProcedure.DeactivateGuildUsers, conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@Users", SqlDbType.Structured).Value = CreateGuildUserIdTable(users);

            await cmd.ExecuteNonQueryAsync();
        }

        private static DataTable CreateUserIdTable(Dictionary<ulong, User> users)
        {
            var table = new DataTable();
            table.Columns.Add("user_id", typeof(long));

            foreach (var user in users.Values)
            {
                table.Rows.Add(user.UserId);
            }

            return table;
        }

        private static DataTable CreateUserDataTable(Dictionary<ulong, User> users)
        {
            var table = new DataTable();
            table.Columns.Add("user_id", typeof(long));
            table.Columns.Add("username", typeof(string));
            table.Columns.Add("is_bot", typeof(bool));
            table.Columns.Add("avatar_url", typeof(string));

            foreach (var user in users.Values)
            {
                table.Rows.Add(user.UserId, user.Username, user.IsBot, user.AvatarUrl);
            }

            return table;
        }

        private static DataTable CreateGuildUserDataTable(Dictionary<ulong, List<GuildUser>> users)
        {
            var table = new DataTable();
            table.Columns.Add("guild_id", typeof(long));
            table.Columns.Add("user_id", typeof(long));
            table.Columns.Add("nickname", typeof(string));
            table.Columns.Add("joined_at", typeof(DateTime));
            table.Columns.Add("is_inactive", typeof(bool));

            foreach (var (guildId, guildUsers) in users)
            {
                foreach (var user in guildUsers)
                {
                    table.Rows.Add(guildId, user.UserId, user.Nickname ?? (object)DBNull.Value, user.JoinedDate, false);
                }
            }

            return table;
        }

        private static DataTable CreateGuildUserIdTable(Dictionary<ulong, List<GuildUser>> users)
        {
            var table = new DataTable();
            table.Columns.Add("user_id", typeof(long));
            table.Columns.Add("guild_id", typeof(long));

            foreach (var (guildId, guildUsers) in users)
            {
                foreach (var user in guildUsers)
                {
                    table.Rows.Add(user.UserId, guildId);
                }
            }

            return table;
        }
    }
}
