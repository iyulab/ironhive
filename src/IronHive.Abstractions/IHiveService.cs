using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Collections;
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
    IProviderCollection Providers { get; }

    /// <summary>
    /// 스토리지 서비스 공급자
    /// </summary>
    IStorageCollection Storages { get; }

    /// <summary>
    /// 툴 서비스 공급자
    /// </summary>
    IToolCollection Tools { get; }

    /// <summary>
    /// 에이전트 서비스
    /// </summary>
    IAgentService Agent { get; }

    /// <summary>
    /// 파일 서비스
    /// </summary>
    IFileService File { get; }

    /// <summary>
    /// 메모리 서비스
    /// </summary>
    IMemoryService Memory { get; }
}
