namespace IronHive.Abstractions.Pipelines;

/// <summary>
/// 타입을 안전하게 캐스팅하기 위해 내부에서 사용되는 Wrapper입니다.
/// </summary>
internal sealed class ObjectPipeline<TInput, TOutput> : IPipeline<object, object>
{
    private readonly IPipeline<TInput, TOutput> _inner;

    public ObjectPipeline(IPipeline<TInput, TOutput> inner)
        => _inner = inner;

    public async Task<object> InvokeAsync(object input, CancellationToken ct = default)
    {
        if (input is not TInput typed)
            throw new InvalidCastException(
                $"Pipeline expected input of type {typeof(TInput).FullName}, but got {input?.GetType().FullName ?? "null"}");

        TOutput result = await _inner.InvokeAsync(typed, ct);
        return (object)result!;
    }
}

/// <summary>
/// 함수(델리게이트)를 감싸는 파이프라인 Wrapper입니다.
/// </summary>
internal sealed class DelegatePipeline<TInput, TOutput> : IPipeline<TInput, TOutput>
{
    private readonly Func<TInput, CancellationToken, Task<TOutput>> _function;

    public DelegatePipeline(Func<TInput, CancellationToken, Task<TOutput>> function)
        => _function = function;

    public DelegatePipeline(Func<TInput, Task<TOutput>> function)
        : this((input, ct) => function(input)) { }

    public DelegatePipeline(Func<TInput, TOutput> function)
        : this((input, ct) => Task.FromResult(function(input))) { }

    public Task<TOutput> InvokeAsync(TInput input, CancellationToken cancellationToken = default)
        => _function(input, cancellationToken);
}

/// <summary>
/// DI 컨테이너에서 파이프라인을 생성해서 사용되는 Wrapper입니다.
/// </summary>
internal sealed class ServiceFactoryPipeline<TInput, TOutput> : IPipeline<TInput, TOutput>
{
    private readonly IServiceProvider _services;
    private readonly Func<IServiceProvider, IPipeline<TInput, TOutput>> _factory;

    public ServiceFactoryPipeline(
        IServiceProvider services,
        Func<IServiceProvider, IPipeline<TInput, TOutput>> factory)
    {
        _services = services;
        _factory = factory;
    }

    public Task<TOutput> InvokeAsync(
        TInput input,
        CancellationToken cancellationToken = default)
    {
        var pipeline = _factory(_services);
        return pipeline.InvokeAsync(input, cancellationToken);
    }
}

/// <summary>
/// DI 컨테이너에서 파이프라인 훅을 생성해서 사용되는 Wrapper입니다.
/// </summary>
internal sealed class ServiceFactoryPipelineHook : IPipelineHook
{
    private readonly IServiceProvider _services;
    private readonly Func<IServiceProvider, IPipelineHook> _factory;

    public ServiceFactoryPipelineHook(
        IServiceProvider services,
        Func<IServiceProvider, IPipelineHook> factory)
    {
        _services = services;
        _factory = factory;
    }

    public ValueTask AfterAsync(string name, object output, CancellationToken cancellationToken = default)
        => _factory(_services).AfterAsync(name, output, cancellationToken);

    public ValueTask BeforeAsync(string name, object input, CancellationToken cancellationToken = default)
        => _factory(_services).BeforeAsync(name, input, cancellationToken);

    public ValueTask CancelAsync(string name, CancellationToken cancellationToken)
        => _factory(_services).CancelAsync(name, cancellationToken);

    public ValueTask ErrorAsync(string name, Exception exception, CancellationToken cancellationToken = default)
        => _factory(_services).ErrorAsync(name, exception, cancellationToken);
}