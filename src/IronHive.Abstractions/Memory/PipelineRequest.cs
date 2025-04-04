namespace IronHive.Abstractions.Memory;

/// <summary>
/// 파이프라인 작업 요청을 나타내는 클래스입니다.
/// 이 클래스는 작업 수행에 필요한 원본, 대상, 단계 및 핸들러 옵션을 하나의 객체로 캡슐화합니다.
/// </summary>
public class PipelineRequest
{
    /// <summary>
    /// 작업의 원본 메모리 타겟을 지정합니다.
    /// </summary>
    public required IMemorySource Source { get; set; }

    /// <summary>
    /// 작업 결과를 저장할 대상 메모리 타겟을 지정합니다.
    /// </summary>
    public required IMemoryTarget Target { get; set; }

    /// <summary>
    /// 작업 수행 시 실행할 단계들의 목록입니다. 실행 순서대로 정렬되어야 합니다.
    /// </summary>
    public required IEnumerable<string> Steps { get; set; }

    /// <summary>
    /// 작업 핸들러에 전달할 추가 옵션들을 포함하는 딕셔너리입니다.
    /// 옵션이 필요한 경우에만 사용하며, 선택적으로 제공할 수 있습니다.
    /// </summary>
    public IDictionary<string, object?>? HandlerOptions { get; set; }
}
