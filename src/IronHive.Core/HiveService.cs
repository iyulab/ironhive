using IronHive.Abstractions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Models;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Images;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Videos;
using IronHive.Abstractions.Audio;
using IronHive.Abstractions.Vector;
using IronHive.Abstractions.Queue;
using IronHive.Core.Memory;
using IronHive.Core.Agent;

namespace IronHive.Core;

public class HiveService : IHiveService
{
    private readonly AgentService _agents;
    private readonly IReadOnlyDictionary<string, IVectorStorage> _vectors;
    private readonly IReadOnlyDictionary<string, IQueueStorage> _queues;

    internal HiveService(
        IModelService models,
        IMessageService messages,
        IEmbeddingService embeddings,
        IImageService images,
        IVideoService videos,
        IAudioService audio,
        IFileStorageService files,
        IReadOnlyDictionary<string, IVectorStorage> vectors,
        IReadOnlyDictionary<string, IQueueStorage> queues)
    {
        Models = models;
        Messages = messages;
        Embeddings = embeddings;
        Images = images;
        Videos = videos;
        Audio = audio;
        Files = files;
        Memory = new MemoryService(vectors, queues, embeddings);
        _agents = new AgentService(messages);
        _vectors = vectors;
        _queues = queues;
    }

    public IModelService Models { get; }
    public IMessageService Messages { get; }
    public IEmbeddingService Embeddings { get; }
    public IImageService Images { get; }
    public IVideoService Videos { get; }
    public IAudioService Audio { get; }
    public IFileStorageService Files { get; }
    public IMemoryService Memory { get; }

    public IAgent CreateAgentFrom(Action<AgentConfig> configure)
        => _agents.CreateAgent(configure);

    public IAgent CreateAgentFrom(AgentCard card)
    {
        ArgumentNullException.ThrowIfNull(card);
        if (string.IsNullOrWhiteSpace(card.Workflow))
            throw new ArgumentException("AgentCard.Workflow is required.", nameof(card));
        return _agents.CreateAgentFromYaml(card.Workflow);
    }

    public IAgent CreateAgentFromYaml(string yaml)
        => _agents.CreateAgentFromYaml(yaml);

    public IMemoryWorker CreateMemoryWorker(
        Func<MemoryWorkerBuilder, MemoryPipelineBuilder> configure,
        IServiceProvider? sp = null)
    {
        var builder = new MemoryWorkerBuilder(_vectors, _queues, sp);
        return configure(builder).Build();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

public static class HiveServiceExtensions
{
    public static IMemoryWorker CreateMemoryWorkerFrom(
        this IHiveService service,
        Func<MemoryWorkerBuilder, MemoryPipelineBuilder> configure,
        IServiceProvider? sp = null)
    {
        if (service is HiveService hs)
            return hs.CreateMemoryWorker(configure, sp);
        throw new NotSupportedException($"CreateMemoryWorkerFrom is not supported by {service.GetType().Name}");
    }
}