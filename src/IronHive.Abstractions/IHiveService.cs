using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Memory;

namespace IronHive.Abstractions;

/// <summary>
/// HiveService 인터페이스
/// </summary>
public interface IHiveService
{
    /// <summary>
    /// 서비스 제공자
    /// </summary>
    IServiceProvider Services { get; }

    /// <summary>
    /// AI 서비스 공급자
    /// </summary>
    IKeyedCollectionGroup<IKeyedProvider> Providers { get; }

    /// <summary>
    /// 스토리지 서비스 공급자
    /// </summary>
    IKeyedCollectionGroup<IKeyedStorage> Storages { get; }

    /// <summary>
    /// 에이전트 서비스
    /// </summary>
    IAgentService Agents { get; }

    /// <summary>
    /// 파일 서비스
    /// </summary>
    IFileService Files { get; }

    /// <summary>
    /// 메모리 서비스
    /// </summary>
    IMemoryService Memory { get; }
}
