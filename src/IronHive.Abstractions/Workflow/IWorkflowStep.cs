namespace IronHive.Abstractions.Workflow;

/// <summary>
/// 워크플로우의 공통 추상화 마커 인터페이스 입니다.
/// </summary>
public interface IWorkflowStep
{ }

/// <summary>
/// 옵션을 받는 워크플로우 작업(태스크) 단계의 계약을 정의합니다.
/// </summary>
/// <typeparam name="TContext">실행 시 주입되는 컨텍스트 타입</typeparam>
/// <typeparam name="TOptions">실행에 필요한 옵션/파라미터 DTO 타입.</typeparam>
public interface IWorkflowTask<TContext, TOptions> : IWorkflowStep
{
    /// <summary>
    /// 작업을 비동기로 실행합니다.
    /// </summary>
    /// <param name="context">현재 실행 컨텍스트.</param>
    /// <param name="options">태스크별 실행 옵션.</param>
    /// <param name="cancellationToken">작업 취소 토큰.</param>
    /// <returns>
    /// <see cref="TaskStepResult"/>를 반환합니다.
    /// 성공 시 필요한 경우 Data/Message에 결과를 채울 수 있으며,
    /// 실패 시 예외를 던지지 않고 Fail 상태를 반환하는 것도 가능합니다.
    /// </returns>
    Task<TaskStepResult> ExecuteAsync(
        TContext context,
        TOptions options,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 별도의 옵션이 필요 없는 워크플로우 작업(태스크) 단계의 계약을 정의합니다.
/// </summary>
/// <typeparam name="TContext">실행 컨텍스트 타입.</typeparam>
public interface IWorkflowTask<TContext> : IWorkflowTask<TContext, object?>
{
    /// <summary>
    /// 옵션 없이 태스크를 비동기로 실행합니다.
    /// </summary>
    /// <param name="context">현재 실행 컨텍스트.</param>
    /// <param name="cancellationToken">작업 취소 토큰.</param>
    /// <returns>
    /// <see cref="TaskStepResult"/>를 반환합니다.
    /// 실패 처리 전략은 구현/정책에 따릅니다
    /// </returns>
    Task<TaskStepResult> ExecuteAsync(
        TContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <see cref="IWorkflowTask{TContext, TOptions}"/>의 명시적 구현입니다.
    /// </summary>
    Task<TaskStepResult> IWorkflowTask<TContext, object?>.ExecuteAsync(TContext context, object? _, CancellationToken ct)
        => ExecuteAsync(context, ct);
}

/// <summary>
/// 분기 조건을 평가하는 단계의 계약을 정의합니다.
/// </summary>
/// <typeparam name="TContext">평가에 사용되는 컨텍스트 타입.</typeparam>
public interface IWorkflowCondition<TContext> : IWorkflowStep
{
    /// <summary>
    /// 분기 키(예: "Approved", "Rejected", "Retry")를 비동기로 평가/반환합니다.
    /// </summary>
    /// <param name="context">현재 실행 컨텍스트.</param>
    /// <param name="cancellationToken">작업 취소 토큰.</param>
    /// <returns>
    /// <see cref="ConditionStepResult"/>를 반환합니다.
    /// 결과에는 선택된 분기 키를 포함해야 하며, 분기 키는 라우팅/다음 스텝 선택에 사용됩니다.
    /// 미정의 키를 반환할 경우 상위 워크플로우에서 기본 경로/오류 정책을 적용해야 합니다.
    /// </returns>
    Task<ConditionStepResult> EvaluateAsync(
        TContext context,
        CancellationToken cancellationToken = default);
}
