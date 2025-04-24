using DataAccessLayerInterfaces;
using Microsoft.Data.SqlClient;

namespace DataAccessLayer
{
    public class DBConnection : IDBConnection
    {
        public SqlConnection GetConnection()
        {
            var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
            var conn = new SqlConnection(connectionString);

            return conn;
        }
    }
}
