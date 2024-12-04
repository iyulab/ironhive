namespace Raggle.Server.Configurations;

public class RaggleConfig
{
    public RaggleDatabaseConfig Database { get; set; } = new RaggleDatabaseConfig();

    public RaggleStorageConfig Storages { get; set; } = new RaggleStorageConfig();

    public RaggleKeyedServiceConfig Services { get; set; } = new RaggleKeyedServiceConfig();
}
