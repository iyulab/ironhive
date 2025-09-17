namespace IronHive.Abstractions.Workflow;

/// <summary>
/// 특정 <typeparamref name="TContext"/> 컨텍스트를 기반으로 워크플로를 전체 실행하거나,
/// 지정한 노드부터 이어서 실행할 수 있습니다.
/// </summary>
/// <typeparam name="TContext">
/// 워크플로 실행 동안 공유되는 상태/데이터를 담는 컨텍스트 타입입니다.
/// </typeparam>
public interface IWorkflow<TContext>
{
    /// <summary>
    /// 워크플로의 표시용 이름입니다. (선택)
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// 워크플로의 버전입니다. (선택)
    /// </summary>
    public Version? Version { get; }

    /// <summary>
    /// 워크플로의 진행 상태가 변경되었을 때 발생하는 이벤트입니다.
    /// 예: 노드 진입/종료, 전이(transition) 완료, 체크포인트 생성 등.
    /// </summary>
    public event EventHandler<WorkflowEventArgs<TContext>>? Progressed;

    /// <summary>
    /// 루트(초기) 노드부터 워크플로를 비동기적으로 전체 실행합니다.
    /// </summary>
    /// <param name="context">실행에 사용될 컨텍스트 인스턴스입니다. <c>null</c>일 수 없습니다.</param>
    /// <param name="cancellationToken">실행을 취소하기 위한 토큰입니다. (선택)</param>
    /// <returns>실행 완료를 나타내는 <see cref="Task"/>를 반환합니다.</returns>
    Task RunAsync(
        TContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 노드 ID에서부터 워크플로를 비동기적으로 이어서 실행합니다.
    /// </summary>
    /// <param name="nodeId">실행을 시작할 노드의 식별자입니다. <c>null</c>이거나 빈 문자열일 수 없습니다.</param>
    /// <param name="context">실행에 사용될 컨텍스트 인스턴스입니다. <c>null</c>일 수 없습니다.</param>
    /// <param name="cancellationToken">실행을 취소하기 위한 토큰입니다. (선택)</param>
    /// <returns>실행 완료를 나타내는 <see cref="Task"/>를 반환합니다.</returns>
    Task RunFromAsync(
        string nodeId,
        TContext context,
        CancellationToken cancellationToken = default);
}
