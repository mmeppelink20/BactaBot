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
#if DEBUG
            var connectionString = _configuration["profiles:BactaBot:environmentVariables:STAGE_DATABASE_CONNECTION_STRING"];
#else
            var connectionString = _configuration["profiles:BactaBot:environmentVariables:PROD_DATABASE_CONNECTION_STRING"];
#endif
            var conn = new SqlConnection(connectionString);

            return conn;
        }
    }
}
