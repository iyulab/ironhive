namespace Raggle.Server.Configurations.Models;

public enum DatabaseTypes
{
    Sqlite,
    SqlServer,
    Cosmos,
    MySql,
    PostgreSQL,
    Oracle,
    MongoDB
}

public partial class RaggleDatabaseConfig
{
    public DatabaseTypes Type { get; set; }

    public SqliteConfig Sqlite { get; set; } = new SqliteConfig();

    public SqlServerConfig SqlServer { get; set; } = new SqlServerConfig();

    public CosmosConfig Cosmos { get; set; } = new CosmosConfig();

    public PostgreSQLConfig PostgreSQL { get; set; } = new PostgreSQLConfig();

    public MongoDBConfig MongoDB { get; set; } = new MongoDBConfig();

    public MySqlConfig MySql { get; set; } = new MySqlConfig();

    public OracleConfig Oracle { get; set; } = new OracleConfig();

    public class SqliteConfig
    {
        public string ConnectionString { get; set; } = string.Empty;
    }

    public class SqlServerConfig
    {
        public string ConnectionString { get; set; } = string.Empty;
    }

    public class CosmosConfig
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
    }

    public class PostgreSQLConfig
    {
        public string ConnectionString { get; set; } = string.Empty;
    }

    public class MongoDBConfig
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
    }

    public class MySqlConfig
    {
        public string ConnectionString { get; set; } = string.Empty;
    }

    public class OracleConfig
    {
        public string ConnectionString { get; set; } = string.Empty;
    }
}
