using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Models;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Images;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Videos;
using IronHive.Abstractions.Audio;

namespace IronHive.Abstractions;

public interface IHiveService : IDisposable
{
    IModelService Models { get; }
    IMessageService Messages { get; }
    IEmbeddingService Embeddings { get; }
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
    /// <paramref name="provider"/>를 지정하지 않으면 단일 등록된 provider가 자동 선택되고,
    /// 둘 이상 등록돼 있으면 예외가 발생합니다(<see cref="Messages"/>가 요청별로 라우팅하는
    /// 것과 같은 규칙). M.E.AI 연동(<c>AsChatClient</c> 등) 등 provider 하나에 직접 바인딩된
    /// 컴포넌트가 필요할 때 씁니다.
    /// </summary>
    IMessageGenerator GetMessageGenerator(string? provider = null);

    /// <summary>
    /// 등록된 provider의 raw <see cref="IEmbeddingGenerator"/>를 가져옵니다.
    /// <paramref name="provider"/>를 지정하지 않으면 단일 등록된 provider가 자동 선택되고,
    /// 둘 이상 등록돼 있으면 예외가 발생합니다. M.E.AI 연동(<c>AsEmbeddingGenerator</c> 등)
    /// 등 provider 하나에 직접 바인딩된 컴포넌트가 필요할 때 씁니다.
    /// </summary>
    IEmbeddingGenerator GetEmbeddingGenerator(string? provider = null);
}
