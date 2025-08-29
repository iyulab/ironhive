using IronHive.Abstractions.Pipelines;
using IronHive.Abstractions.Storages;

namespace IronHive.Abstractions.Memory;

/// <summary>
/// 메모리 서비스 빌더 인터페이스입니다.
/// </summary>
public interface IMemoryServiceBuilder
{
    /// <summary>
    /// 벡터 저장소를 설정합니다.
    /// </summary>
    IMemoryServiceBuilder SetVectorStorage(
        IVectorStorage storage);

    /// <summary>
    /// 큐 저장소를 설정합니다.
    /// </summary>
    IMemoryServiceBuilder SetQueueStorage(
        IQueueStorage<MemoryPipelineContext<object>> storage);

    /// <summary>
    /// 메모리 파이프라인을 설정합니다.
    /// </summary>
    IMemoryServiceBuilder SetPipeline();

    /// <summary>
    /// 메모리 서비스를 빌드합니다.
    /// </summary>
    IMemoryService Build();
}
