namespace IronHive.Abstractions.Pipelines;

/// <summary>
/// 파이프라인 단계 실행의 수명주기에 후킹하기 위한 인터페이스입니다.
/// 각 메서드는 비동기로 호출되며, 구현체는 I/O 여부에 따라 동기/비동기 모두 가능하게 작성할 수 있습니다.
/// </summary>
/// <remarks>
/// • 메서드는 예외를 던지지 않고 가능하면 자체 처리/로깅만 수행하는 것을 권장합니다.
/// • 동기 작업만 필요한 경우 ValueTask.CompletedTask를 반환하세요.
/// </remarks>
public interface IPipelineHook
{
    /// <summary>
    /// 파이프라인 단계가 실행되기 <b>직전</b>에 호출됩니다.
    /// </summary>
    /// <param name="name">현재 실행될 단계의 고유 이름.</param>
    /// <param name="input">해당 단계에 전달될 입력 객체.</param>
    /// <param name="cancellationToken">취소 토큰.</param>
    ValueTask BeforeAsync(
        string name,
        object input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 파이프라인 단계가 <b>성공적으로</b> 실행된 직후 호출됩니다.
    /// </summary>
    /// <param name="name">방금 실행이 완료된 단계의 고유 이름.</param>
    /// <param name="output">해당 단계의 출력 객체(다음 단계 입력으로 전달됨).</param>
    /// <param name="cancellationToken">취소 토큰.</param>
    ValueTask AfterAsync(
        string name,
        object output,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 파이프라인 단계 실행 중 예외가 발생했을 때 호출됩니다.
    /// </summary>
    /// <param name="name">예외가 발생한 단계의 고유 이름.</param>
    /// <param name="exception">발생한 예외.</param>
    /// <param name="cancellationToken">취소 토큰.</param>
    ValueTask ErrorAsync(
        string name,
        Exception exception,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 파이프라인 실행이 취소되었을 때 호출됩니다.
    /// </summary>
    /// <param name="name">취소가 발생한 단계 이름.</param>
    /// <param name="cancellationToken">취소를 알린 토큰.</param>
    ValueTask CancelAsync(
        string name,
        CancellationToken cancellationToken);
}