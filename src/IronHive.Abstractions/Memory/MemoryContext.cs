namespace IronHive.Abstractions.Memory;

/// <summary>
/// 메모리 작업을 처리하는 파이프라인의 컨텍스트입니다.
/// </summary>
public class MemoryContext
{
    /// <summary>
    /// 메모리 작업의 대상이 되는 소스입니다.
    /// </summary>
    public required IMemorySource Source { get; set; }

    /// <summary>
    /// 메모리 작업의 대상이 되는 타겟입니다.
    /// </summary>
    public required IMemoryTarget Target { get; set; }

    /// <summary>
    /// 파이프라인 단계 간 공유되는 데이터입니다. 키는 대소문자를 구분합니다.
    /// </summary>
    public IDictionary<string, object?> Payload { get; } =
        new Dictionary<string, object?>(StringComparer.Ordinal);
}
