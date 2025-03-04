namespace Raggle.Stack.WebApi.Configurations;

public partial class ServiceConfig
{
    public DatabaseConfig Database { get; set; } = new DatabaseConfig();

    public StorageConfig Storages { get; set; } = new StorageConfig();

    public AIServiceConfig Services { get; set; } = new AIServiceConfig();
}
