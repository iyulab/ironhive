using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Models;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Images;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Reranking;
using IronHive.Abstractions.Videos;
using IronHive.Abstractions.Audio;

namespace IronHive.Abstractions;

public interface IHiveService : IDisposable
{
    IModelService Models { get; }
    IMessageService Messages { get; }
    IEmbeddingService Embeddings { get; }
    IRerankService Rerank { get; }
    IImageService Images { get; }
    IVideoService Videos { get; }
    IAudioService Audio { get; }
    IFileStorageService Files { get; }
    IMemoryService Memory { get; }

    IAgent CreateAgentFrom(Action<AgentConfig> configure);
    IAgent CreateAgentFrom(AgentCard card);
    IAgent CreateAgentFromYaml(string yaml);

    /// <summary>
    /// 등록된 provider의 raw <see cref="IMessageGenerator"/>를 가져옵니다.
    /// </summary>
    /// <remarks>
    /// 대신 <c>Messages.Generators</c>를 쓰세요 — <c>Messages.Generators.GetOrFirstValue(provider)</c>가
    /// 동일하게 동작합니다.
    /// </remarks>
    [Obsolete("Use Messages.Generators (with the GetOrFirstValue extension) instead. Will be removed in a future release.")]
    IMessageGenerator GetMessageGenerator(string? provider = null);

    /// <summary>
    /// 등록된 provider의 raw <see cref="IEmbeddingGenerator"/>를 가져옵니다.
    /// </summary>
    /// <remarks>
    /// 대신 <c>Embeddings.Generators</c>를 쓰세요 — <c>Embeddings.Generators.GetOrFirstValue(provider)</c>가
    /// 동일하게 동작합니다.
    /// </remarks>
    [Obsolete("Use Embeddings.Generators (with the GetOrFirstValue extension) instead. Will be removed in a future release.")]
    IEmbeddingGenerator GetEmbeddingGenerator(string? provider = null);
}
