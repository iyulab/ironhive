namespace IronHive.Abstractions.Memory;

/// <summary>
/// 파이프라인 진행 상태를 나타내는 열거형입니다.
/// </summary>
public enum PipelineStatus
{
    /// <summary>
    /// 작업이 시작되었습니다.
    /// </summary>
    Started,

    /// <summary>
    /// 데이터 처리가 시작되기 전입니다.
    /// </summary>
    ProcessBefore,

    /// <summary>
    /// 데이터 처리가 완료되었습니다.
    /// </summary>
    ProcessAfter,

    /// <summary>
    /// 파이프라인 처리가 완료되었습니다.
    /// </summary>
    Completed,

    /// <summary>
    /// 파이프라인 처리중 오류가 발생했습니다.
    /// </summary>
    Failed
}

/// <summary>
/// 메모리 파이프라인 진행 이벤트에 대한 인자입니다.
/// </summary>
public class MemoryPipelineEventArgs : EventArgs
{
    public MemoryPipelineEventArgs(PipelineStatus status, MemoryPipelineContext context)
    {
        Status = status;
        Context = context;
    }

    /// <summary>
    /// 현재 진행 상태입니다.
    /// </summary>
    public PipelineStatus Status { get; }

    /// <summary>
    /// 현재 파이프라인 컨텍스트입니다.
    /// </summary>
    public MemoryPipelineContext Context { get; }
}

/// <summary>
/// 메모리 파이프라인 오류 이벤트에 대한 인자입니다.
/// </summary>
public class MemoryPipelineErrorEventArgs : MemoryPipelineEventArgs
{
    public MemoryPipelineErrorEventArgs(MemoryPipelineContext context, Exception exception)
        : base(PipelineStatus.Failed, context)
    {
        Exception = exception;
    }

    /// <summary>
    /// 발생한 예외입니다.
    /// </summary>
    public Exception Exception { get; }
}
