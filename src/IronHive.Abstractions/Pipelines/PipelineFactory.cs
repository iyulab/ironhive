namespace IronHive.Abstractions.Pipelines;

/// <summary>
/// 파이프라인 빌더를 생성하기 위한 경량 정적 팩토리입니다.
/// </summary>
public static class PipelineFactory
{
    /// <summary>
    /// 입력<typeparamref name="TInput"/>으로 시작하는 파이프라인 빌더를 생성합니다.
    /// </summary>
    /// <typeparam name="TInput">파이프라인의 최초 입력 타입입니다.</typeparam>
    /// <param name="services">
    /// 선택 사항: 단계/훅을 DI로 해결할 때 사용할 <see cref="IServiceProvider"/> 입니다.
    /// null인 경우 서비스 등록 시 <see cref="InvalidOperationException"/>이 발생할 수 있습니다.
    /// </param>
    /// <returns>
    /// 체이닝을 통해 단계를 추가하고 마지막에 실행기를 만들 수 있는 빌더를 반환합니다.
    /// </returns>
    /// <example>
    /// <code>
    /// var pipeline = PipelineFactory.Create&lt;TInput&gt;(services)
    ///     .Use(new PipelineHook())              // Pipeline의 훅 등록
    ///     .Add("first", new FirstPipeline())    // Pipeline 인스턴스 등록
    ///     .Add("second", (input, ct) => { /* ... */ }) // 델리게이트 등록
    ///     .Build();                             // 실행기 생성
    ///
    /// await pipeline.InvokeAsync(input);        // 파이프라인 실행
    /// </code>
    /// </example>
    public static IPipelineBuilder<TInput, TInput> Create<TInput>(IServiceProvider? services = null)
        => PipelineBuilder<TInput, TInput>.Start<TInput>(services);
}
