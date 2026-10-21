using IronHive.Abstractions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Models;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Images;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Reranking;
using IronHive.Abstractions.Videos;
using IronHive.Abstractions.Audio;
using IronHive.Core.Agent;

namespace IronHive.Core;

public class HiveService : IHiveService
{
    private readonly AgentService _agents;

    internal HiveService(
        IModelService models,
        IMessageService messages,
        IEmbeddingService embeddings,
        IRerankService rerank,
        IImageService images,
        IVideoService videos,
        IAudioService audio,
        IFileStorageService files)
    {
        Models = models;
        Messages = messages;
        Embeddings = embeddings;
        Rerank = rerank;
        Images = images;
        Videos = videos;
        Audio = audio;
        Files = files;
        _agents = new AgentService(messages);
    }

    public IModelService Models { get; }
    public IMessageService Messages { get; }
    public IEmbeddingService Embeddings { get; }
    public IRerankService Rerank { get; }
    public IImageService Images { get; }
    public IVideoService Videos { get; }
    public IAudioService Audio { get; }
    public IFileStorageService Files { get; }

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

    [Obsolete("Use Messages.Generators (with the GetOrFirstValue extension) instead. Will be removed in a future release.")]
    public IMessageGenerator GetMessageGenerator(string? provider = null)
        => Messages.Generators.GetOrFirstValue(provider);

    [Obsolete("Use Embeddings.Generators (with the GetOrFirstValue extension) instead. Will be removed in a future release.")]
    public IEmbeddingGenerator GetEmbeddingGenerator(string? provider = null)
        => Embeddings.Generators.GetOrFirstValue(provider);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
