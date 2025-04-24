using Microsoft.Data.SqlClient;

namespace DataAccessLayerInterfaces
{
    public interface IDBConnection
    {
        SqlConnection GetConnection();
    }
}
