namespace Raggle.Server.Configurations.Models;

public partial class RaggleConfig
{
    public RaggleDatabaseConfig Database { get; set; } = new RaggleDatabaseConfig();

    public RaggleStorageConfig Storages { get; set; } = new RaggleStorageConfig();

    public RaggleAIConfig Services { get; set; } = new RaggleAIConfig();
}
