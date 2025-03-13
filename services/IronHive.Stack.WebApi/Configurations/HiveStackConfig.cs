namespace IronHive.Stack.WebApi.Configurations;

public class HiveStackConfig
{
    public class DatabaseConfig
    {
        public DatabaseTypes Type { get; set; }

        public string ConnectionString { get; set; } = string.Empty;
    }

    public ServiceKeysConfig ServiceKeys { get; set; } = new ServiceKeysConfig();

    public StoragesConfig Storages { get; set; } = new StoragesConfig();

    public ConnectorsConfig Connectors { get; set; } = new ConnectorsConfig();
}

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
