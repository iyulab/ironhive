namespace IronHive.Abstractions.Memory;

/// <summary>
/// 파이프라인 이벤트를 처리하는 핸들러 인터페이스입니다.
/// </summary>
public interface IPipelineEventHandler
{
    /// <summary>
    /// 파이프라인 시작 시 호출됩니다.
    /// </summary>
    Task OnStartedAsync(PipelineContext context);

    /// <summary>
    /// 파이프라인의 각 단계에서 호출됩니다.
    /// </summary>
    Task OnStepedAsync(PipelineContext context);

    /// <summary>
    /// 파이프라인 완료 시 호출됩니다.
    /// </summary>
    Task OnCompletedAsync(PipelineContext context);

    /// <summary>
    /// 파이프라인 실패 시 호출됩니다.
    /// </summary>
    Task OnFailedAsync(PipelineContext context, Exception exception);
}
