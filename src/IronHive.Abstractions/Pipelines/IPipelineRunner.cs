namespace IronHive.Abstractions.Pipelines;

/// <summary>
/// 구성된 여러 파이프라인 단계를 순차적으로 실행할 수 있는 실행기입니다.
/// </summary>
/// <typeparam name="TInput">실행기에 전달되는 최초 입력 타입.</typeparam>
/// <typeparam name="TOutput">모든 단계를 통과한 최종 출력 타입.</typeparam>
public interface IPipelineRunner<in TInput, TOutput> : IPipeline<TInput, TOutput>
{
    /// <summary>
    /// 지정된 단계 이름에서 시작하여 나머지 단계를 순차적으로 실행합니다.
    /// </summary>
    /// <param name="name">시작할 단계의 고유 이름.</param>
    /// <param name="input">해당 시작 단계에 전달할 입력 값.</param>
    /// <param name="cancellationToken">작업 취소를 위한 토큰.</param>
    /// <returns>최종 출력 값을 담은 <see cref="Task{TResult}"/>.</returns>
    Task<TOutput> InvokeFromAsync(
        string name,
        TInput input,
        CancellationToken cancellationToken = default);
}
