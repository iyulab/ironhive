using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Abstractions.Pipelines;

/// <inheritdoc />
public class PipelineBuilder<TInput, TOutput> : IPipelineBuilder<TInput, TOutput>
{
    private readonly IReadOnlyList<PipelineStep> _steps;
    private readonly IPipelineHook? _hook;
    private readonly IServiceProvider? _services;

    private PipelineBuilder(IReadOnlyList<PipelineStep> steps, IPipelineHook? hook, IServiceProvider? services)
    {
        _steps = steps;
        _hook = hook;
        _services = services;
    }

    /// <summary>
    /// PipelineBuilder의 진입점입니다.
    /// </summary>
    public static IPipelineBuilder<TStart, TStart> Start<TStart>(IServiceProvider? services = null)
        => new PipelineBuilder<TStart, TStart>(Array.Empty<PipelineStep>(), null, services);

    /// <inheritdoc />
    public IPipelineBuilder<TInput, TNext> Add<TNext>(
        string name, 
        IPipeline<TOutput, TNext> pipeline)
    {
        if (string.IsNullOrWhiteSpace(name))
        throw new ArgumentException("Pipeline step must have a non-empty name", nameof(name));

        var steps = _steps.Append(new PipelineStep(name, new ObjectPipeline<TOutput, TNext>(pipeline))).ToList();
        return new PipelineBuilder<TInput, TNext>(steps, _hook, _services);
    }

    /// <inheritdoc />
    public IPipelineBuilder<TInput> Add(
        string name,
        IPipeline<TOutput> pipeline)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Pipeline step must have a non-empty name", nameof(name));

        var finalstep = new PipelineFinalStep(name, new ObjectPipeline<TOutput>(pipeline));
        return new PipelineBuilder<TInput>(_steps, finalstep, _hook);
    }

    /// <inheritdoc />
    public IPipelineBuilder<TInput, TNext> Add<TImplementation, TNext>(
        string name,
        Func<IServiceProvider, TImplementation>? factory = null)
        where TImplementation : class, IPipeline<TOutput, TNext>
    {
        if (_services == null)
            throw new InvalidOperationException("ServiceProvider is required to resolve pipeline by type");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Pipeline step must have a non-empty name", nameof(name));

        factory ??= (sp) => sp.GetRequiredService<TImplementation>();
        var pipeline = new ServiceFactoryPipeline<TOutput, TNext>(_services, factory);
        return Add(name, pipeline);
    }

    /// <inheritdoc />
    public  IPipelineBuilder<TInput> Add<TImplementation>(
        string name, 
        Func<IServiceProvider, TImplementation>? factory)
        where TImplementation : class, IPipeline<TOutput>
    {
        if (_services == null)
            throw new InvalidOperationException("ServiceProvider is required to resolve pipeline by type");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Pipeline step must have a non-empty name", nameof(name));
        factory ??= (sp) => sp.GetRequiredService<TImplementation>();
        var pipeline = new ServiceFactoryPipeline<TOutput>(_services, factory);
        return Add(name, pipeline);
    }

    /// <inheritdoc />
    public IPipelineBuilder<TInput, TNext> Add<TNext>(
        string name,
        Func<TOutput, CancellationToken, Task<TNext>> function)
    {
        return Add(name, new DelegatePipeline<TOutput, TNext>(function));
    }

    /// <inheritdoc />
    public IPipelineBuilder<TInput> Add(
        string name, 
        Func<TOutput, CancellationToken, Task> function)
    {
        return Add(name, new DelegatePipeline<TOutput>(function));
    }

    /// <inheritdoc />
    public IPipelineBuilder<TInput, TOutput> Use(IPipelineHook hook)
    {
        return new PipelineBuilder<TInput, TOutput>(_steps, hook, _services);
    }

    /// <inheritdoc />
    public IPipelineBuilder<TInput, TOutput> Use(Func<IServiceProvider, IPipelineHook>? factory = null)
    {
        if (_services == null)
            throw new InvalidOperationException("ServiceProvider is required to resolve hook by type");
        factory ??= (sp) => sp.GetRequiredService<IPipelineHook>();
        var hook = new ServiceFactoryPipelineHook(_services, factory);
        return new PipelineBuilder<TInput, TOutput>(_steps, hook, _services);
    }

    /// <inheritdoc />
    public IPipelineRunner<TInput, TOutput> Build()
    {
        return new PipelineRunner<TInput, TOutput>(_steps, _hook);
    }
}

/// <inheritdoc />
public class PipelineBuilder<TInput> : IPipelineBuilder<TInput>
{
    private readonly IReadOnlyList<PipelineStep> _steps;
    private readonly PipelineFinalStep _finalStep;
    private readonly IPipelineHook? _hook;

    public PipelineBuilder(IReadOnlyList<PipelineStep> steps, PipelineFinalStep finalStep, IPipelineHook? hook)
    {
        _steps = steps;
        _finalStep = finalStep;
        _hook = hook;
    }

    /// <inheritdoc />
    public IPipelineRunner<TInput> Build()
    {
        return new PipelineRunner<TInput>(_steps, _finalStep, _hook);
    }
}