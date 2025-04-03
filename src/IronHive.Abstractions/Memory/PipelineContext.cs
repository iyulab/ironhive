namespace IronHive.Abstractions.Memory;

/// <summary>
/// 파이프라인 핸들러에 전달되는 데이터 객체입니다.
/// </summary>
public class PipelineContext
{
    /// <summary>
    /// 파이프라인 데이터의 메인 소스입니다.
    /// </summary>
    public IMemorySource? Source { get; set; }

    /// <summary>
    /// 파이프라인의 중간경유 데이터 입니다.
    /// </summary>
    public object? Payload { get; set; }

    /// <summary>
    /// 핸들러의 옵션입니다.
    /// </summary>
    public object? Options { get; set; }

    public PipelineContext()
    { }

    public PipelineContext(IMemorySource source)
    {
        Source = source;
    }
}
