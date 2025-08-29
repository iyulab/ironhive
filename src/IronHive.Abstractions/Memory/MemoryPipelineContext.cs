namespace IronHive.Abstractions.Memory;

/// <summary>
/// 파이프라인 핸들러에 전달되는 데이터 객체입니다.
/// </summary>
public class MemoryPipelineContext<T>
{
    /// <summary>
    /// 파이프라인 데이터의 원본 소스입니다.
    /// </summary>
    public required IMemorySource Source { get; init; }

    /// <summary>
    /// 파이프라인 데이터의 저장경로입니다.
    /// </summary>
    public required IMemoryTarget Target { get; init; }

    /// <summary>
    /// 파이프라인의 중간경유 데이터 입니다.
    /// </summary>
    public T? Payload { get; set; }
}
