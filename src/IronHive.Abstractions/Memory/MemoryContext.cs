namespace IronHive.Abstractions.Memory;

/// <summary>
/// 메모리 작업을 처리하는 파이프라인의 컨텍스트입니다.
/// </summary>
public class MemoryContext
{
    /// <summary>
    /// 임베딩에 사용될 소스입니다.
    /// </summary>
    public required IMemorySource Source { get; set; }

    /// <summary>
    /// 처리된 데이터를 저장할 대상입니다.
    /// </summary>
    public required IMemoryTarget Target { get; set; }

    /// <summary>
    /// 파이프라인의 중간경유 데이터 입니다.
    /// </summary>
    public object? Payload { get; set; }
}