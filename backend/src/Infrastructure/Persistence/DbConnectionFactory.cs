using MySqlConnector;

namespace Infrastructure.Persistence;

public class DbConnectionFactory(string connectionString)
{
    public MySqlConnection CreateConnection() => new(connectionString);
}
