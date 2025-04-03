namespace IronHive.Abstractions.Memory;

/// <summary>
/// 파이프라인 이벤트를 처리하는 핸들러 인터페이스입니다.
/// </summary>
public interface IPipelineEventHandler
{
    /// <summary>
    /// 파이프 라인이 대기 중일 때 호출됩니다.
    /// </summary>
    Task OnQueuedAsync(string pipelineId);

    /// <summary>
    /// 파이프라인의 데이터를 처리하기 전 호출됩니다.
    /// </summary>
    Task OnProcessBeforeAsync(string pipelineId, string step, PipelineContext context);

    /// <summary>
    /// 파이프라인의 데이터를 처리한 후 호출됩니다.
    /// </summary>
    Task OnProcessAfterAsync(string pipelineId, string step, PipelineContext context);

    /// <summary>
    /// 파이프라인 완료 시 호출됩니다.
    /// </summary>
    Task OnCompletedAsync(string pipelineId);

    /// <summary>
    /// 파이프라인 실패 시 호출됩니다.
    /// </summary>
    Task OnFailedAsync(string pipelineId, Exception exception);
}
