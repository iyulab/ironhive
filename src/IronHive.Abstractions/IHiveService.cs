using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Images;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Videos;
using IronHive.Abstractions.Audio;
using IronHive.Abstractions.Workflow;

namespace IronHive.Abstractions;

public interface IHiveService : IAsyncDisposable
{
    IModelCatalogService Catalog { get; }
    IMessageService Messages { get; }
    IEmbeddingService Embeddings { get; }
    IImageService Images { get; }
    IVideoService Videos { get; }
    IAudioService Audio { get; }
    IFileStorageService Files { get; }
    IMemoryService Memory { get; }
    IWorkflowFactory Workflows { get; }

    IAgent CreateAgent(Action<AgentConfig> configure);
    IAgent CreateAgentFrom(AgentCard card);
    IAgent CreateAgentFromYaml(string yaml);
}
