namespace IronHive.Abstractions.Pipelines;

/// <summary>
/// 입력을 받아 비동기적으로 출력으로 변환하는 파이프라인 단계의 최소 단위입니다.
/// </summary>
/// <typeparam name="TInput">파이프라인 단계가 처리할 입력 타입.</typeparam>
/// <typeparam name="TOutput">파이프라인 단계가 생성할 출력 타입.</typeparam>
public interface IPipeline<in TInput, TOutput>
{
    /// <summary>
    /// 파이프라인 단계를 비동기적으로 실행합니다.
    /// </summary>
    /// <param name="input">해당 단계가 처리할 입력 값.</param>
    /// <param name="cancellationToken">작업 취소를 위한 토큰.</param>
    /// <returns>출력 값을 담은 <see cref="Task{TResult}"/>.</returns>
    Task<TOutput> InvokeAsync(
        TInput input,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 입력을 받아 비동기적으로 처리하는 파이프라인 단계의 마지막 단위입니다.
/// </summary>
/// <typeparam name="TInput">파이프라인에서 처리할 입력 타입</typeparam>
public interface IPipeline<in TInput>
{
    /// <summary>
    /// 파이프라인 단계를 비동기적으로 실행합니다.
    /// </summary>
    /// <param name="input">해당 단계가 처리할 입력 값.</param>
    /// <param name="cancellationToken">작업 취소를 위한 토큰.</param>
    /// <returns>작업 완료를 나타내는 <see cref="Task"/>.</returns>
    Task InvokeAsync(
        TInput input, 
        CancellationToken cancellationToken = default);
}