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

namespace IronHive.Core;

public class HiveService : IHiveService
{
    private readonly IAgentService _agents;
    private readonly IReadOnlyDictionary<string, IVectorStorage> _vectorStorages;
    private readonly IReadOnlyDictionary<string, IQueueStorage> _queueStorages;

    internal HiveService(
        IModelService models,
        IMessageService messages,
        IEmbeddingService embeddings,
        IImageService images,
        IVideoService videos,
        IAudioService audio,
        IFileStorageService files,
        IMemoryService memory,
        IAgentService agents,
        IReadOnlyDictionary<string, IVectorStorage> vectorStorages,
        IReadOnlyDictionary<string, IQueueStorage> queueStorages)
    {
        Models = models;
        Messages = messages;
        Embeddings = embeddings;
        Images = images;
        Videos = videos;
        Audio = audio;
        Files = files;
        Memory = memory;
        _agents = agents;
        _vectorStorages = vectorStorages;
        _queueStorages = queueStorages;
    }

    public IModelService Models { get; }
    public IMessageService Messages { get; }
    public IEmbeddingService Embeddings { get; }
    public IImageService Images { get; }
    public IVideoService Videos { get; }
    public IAudioService Audio { get; }
    public IFileStorageService Files { get; }
    public IMemoryService Memory { get; }

    public IAgent CreateAgent(Action<AgentConfig> configure)
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
        var builder = new MemoryWorkerBuilder(_vectorStorages, _queueStorages, sp);
        return configure(builder).Build();
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
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