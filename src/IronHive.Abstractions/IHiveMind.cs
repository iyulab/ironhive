using IronHive.Abstractions.Memory;

namespace IronHive.Abstractions;

/// <summary>
/// HiveMind 인터페이스
/// </summary>
public interface IHiveMind
{
    /// <summary>
    /// HiveMind의 서비스 제공자
    /// </summary>
    IServiceProvider Services { get; }

    /// <summary>
    /// 아직 모름
    /// </summary>
    IHiveSession CreateHiveSession(IHiveAgent master, IDictionary<string, IHiveAgent>? agents);

    /// <summary>
    /// 메모리 서비스 생성
    /// </summary>
    IVectorMemory CreateVectorMemory(VectorMemoryConfig config);

    /// <summary>
    /// 메모리 파이프라인 백그라운드 워커 생성
    /// </summary>
    IPipelineWorker CreatePipelineWorker(PipelineWorkerConfig config);
}
