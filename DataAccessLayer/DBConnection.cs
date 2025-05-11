using DataAccessLayerInterfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DataAccessLayer
{
    public class DBConnection(IConfiguration configuration) : IDBConnection
    {
        private readonly IConfiguration _configuration = configuration;
        public SqlConnection GetConnection()
        {
            var connectionString = _configuration["profiles:BactaBot:environmentVariables:DATABASE_CONNECTION_STRING"];
            var conn = new SqlConnection(connectionString);

            return conn;
        }
    }
}
