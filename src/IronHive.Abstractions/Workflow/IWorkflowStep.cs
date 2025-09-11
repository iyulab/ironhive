namespace IronHive.Abstractions.Workflow;

// 공통 마커 인터페이스
public interface IWorkflowStep
{ }

// 옵션이 있는 Task
public interface IWorkflowTask<TContext, TOptions> : IWorkflowStep
{
    // 반환: 필요시 Data/Message 채움. 실패 시 예외 대신 Fail 반환 가능(정책 선택).
    Task<TaskStepResult> ExecuteAsync(
        TContext context, 
        TOptions options, 
        CancellationToken cancellationToken = default);
}

// 옵션이 필요 없는 Task
public interface IWorkflowTask<TContext> : IWorkflowTask<TContext, object?>
{
    // 반환: 필요시 Data/Message 채움. 실패 시 예외 대신 Fail 반환 가능(정책 선택).
    Task<TaskStepResult> ExecuteAsync(
        TContext context, 
        CancellationToken cancellationToken = default);

    // IWorkflowTask<TContext, object?> 구현
    Task<TaskStepResult> IWorkflowTask<TContext, object?>.ExecuteAsync(TContext context, object? _, CancellationToken ct)
        => ExecuteAsync(context, ct);
}

// 분기 판단용 Condition
public interface IWorkflowCondition<TContext> : IWorkflowStep
{
    // 분기 키(string) 반환
    Task<ConditionStepResult> EvaluateAsync(
        TContext context, 
        CancellationToken cancellationToken = default);
}
