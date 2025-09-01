namespace IronHive.Abstractions.Pipelines;

/// <summary>
/// 제네릭 체이닝 기반의 타입 안전한 파이프라인 빌더를 정의합니다.
/// <para>
/// 빌더는 입력 형식 <typeparamref name="TInput"/> 에서 시작하여 단계별로 출력 형식을 갱신하며,
/// 마지막에 <see cref="Build"/> 를 통해 <see cref="IPipelineRunner{TInput, TOutput}"/> 를 생성합니다.
/// </para>
/// </summary>
/// <typeparam name="TInput">파이프라인의 최초 입력 타입.</typeparam>
/// <typeparam name="TOutput">현재 단계의 출력 타입(체이닝 시 다음 단계의 입력 타입).</typeparam>
public interface IPipelineBuilder<TInput, TOutput>
{
    /// <summary>
    /// 지정한 파이프라인 인스턴스를 단계로 추가합니다.
    /// </summary>
    /// <typeparam name="TNext">해당 단계의 출력 타입.</typeparam>
    /// <param name="name">단계 이름(로깅/추적용). 공백일 수 없습니다.</param>
    /// <param name="pipeline">실행할 파이프라인 인스턴스.</param>
    /// <returns>출력 타입이 <typeparamref name="TNext"/> 로 갱신된 빌더.</returns>
    IPipelineBuilder<TInput, TNext> Add<TNext>(
        string name, 
        IPipeline<TOutput, TNext> pipeline);

    /// <summary>
    /// 지정한 파이프라인 인스턴스를 단계로 추가합니다.
    /// </summary>
    /// <param name="name">단계 이름(로깅/추적용). 공백일 수 없습니다.</param>
    /// <param name="pipeline">실행할 파이프라인 인스턴스.</param>
    IPipelineBuilder<TInput> Add(
        string name,
        IPipeline<TOutput> pipeline);

    /// <summary>
    /// DI 컨테이너로부터 파이프라인을 해결(혹은 팩토리로 생성)하여 단계로 추가합니다.
    /// </summary>
    /// <typeparam name="TImplementation">해결/생성될 파이프라인 구현 타입.</typeparam>
    /// <typeparam name="TNext">해당 단계의 출력 타입.</typeparam>
    /// <param name="name">단계 이름(로깅/추적용). 공백일 수 없습니다.</param>
    /// <param name="factory">
    /// 서비스 제공자에서 구현을 생성하는 팩토리. 지정하지 않으면 <c>IServiceProvider.GetRequiredService&lt;TImplementation&gt;</c> 을 사용합니다.
    /// </param>
    /// <returns>출력 타입이 <typeparamref name="TNext"/> 로 갱신된 빌더.</returns>
    IPipelineBuilder<TInput, TNext> Add<TImplementation, TNext>(
        string name,
        Func<IServiceProvider, TImplementation>? factory = null)
        where TImplementation : class, IPipeline<TOutput, TNext>;

    /// <summary>
    /// DI 컨테이너로부터 파이프라인을 해결(혹은 팩토리로 생성)하여 단계로 추가합니다.
    /// </summary>
    /// <typeparam name="TImplementation">해결/생성될 파이프라인 구현 타입.</typeparam>
    /// <param name="name">단계 이름(로깅/추적용). 공백일 수 없습니다.</param>
    /// <param name="factory">
    /// 서비스 제공자에서 구현을 생성하는 팩토리. 지정하지 않으면 <c>IServiceProvider.GetRequiredService&lt;TImplementation&gt;</c> 을 사용합니다.
    /// </param>
    /// <returns>출력 타입이 Task 로 갱신된 빌더.</returns>
    IPipelineBuilder<TInput> Add<TImplementation>(
        string name,
        Func<IServiceProvider, TImplementation>? factory = null)
        where TImplementation : class, IPipeline<TOutput>;

    /// <summary>
    /// 델리게이트 함수를 단계로 추가합니다.
    /// </summary>
    /// <typeparam name="TNext">해당 단계의 출력 타입.</typeparam>
    /// <param name="name">단계 이름(로깅/추적용). 공백일 수 없습니다.</param>
    /// <param name="function">입력과 취소 토큰을 받아 비동기 결과를 반환하는 함수.</param>
    /// <returns>출력 타입이 <typeparamref name="TNext"/> 로 갱신된 빌더.</returns>
    IPipelineBuilder<TInput, TNext> Add<TNext>(
        string name,
        Func<TOutput, CancellationToken, Task<TNext>> function);

    /// <summary>
    /// 델리게이트 함수를 단계로 추가합니다.
    /// </summary>
    /// <param name="name">단계 이름(로깅/추적용). 공백일 수 없습니다.</param>
    /// <param name="function">입력과 취소 토큰을 받아 비동기 작업을 반환하는 함수.</param>
    IPipelineBuilder<TInput> Add(
        string name,
        Func<TOutput, CancellationToken, Task> function);

    /// <summary>
    /// 파이프라인 실행 전/후를 가로채는 훅을 등록합니다.
    /// </summary>
    /// <param name="hook">훅 구현.</param>
    /// <returns>동일한 출력 타입의 빌더.</returns>
    IPipelineBuilder<TInput, TOutput> Use(IPipelineHook hook);

    /// <summary>
    /// DI 컨테이너(혹은 팩토리)를 통해 훅을 해결하여 등록합니다.
    /// </summary>
    /// <param name="factory">
    /// 서비스 제공자에서 훅을 생성하는 팩토리. 지정하지 않으면 <c>IServiceProvider.GetRequiredService&lt;IPipelineHook&gt;</c> 을 사용합니다.
    /// </param>
    /// <returns>동일한 출력 타입의 빌더.</returns>
    IPipelineBuilder<TInput, TOutput> Use(Func<IServiceProvider, IPipelineHook>? factory = null);

    /// <summary>
    /// 현재까지 구성한 단계와 훅을 기반으로 실행기를 생성합니다.
    /// </summary>
    /// <returns>파이프라인 실행기.</returns>
    IPipelineRunner<TInput, TOutput> Build();
}

/// <summary>
/// 제네릭 체이닝 기반의 타입 안전한 파이프라인 빌더를 정의합니다.
/// <para>
/// 빌더는 입력 형식 <typeparamref name="TInput"/> 에서 시작하여 단계별로 출력 형식을 갱신하며,
/// 마지막에 <see cref="Build"/> 를 통해 <see cref="IPipelineRunner{TOutput}"/> 를 생성합니다.
/// </para>
/// </summary>
/// <typeparam name="TInput">파이프라인의 최초 입력 타입.</typeparam>
public interface IPipelineBuilder<TInput>
{
    /// <summary>
    /// 현재까지 구성한 단계와 훅을 기반으로 실행기를 생성합니다.
    /// </summary>
    /// <returns>파이프라인 실행기.</returns>
    IPipelineRunner<TInput> Build();
}