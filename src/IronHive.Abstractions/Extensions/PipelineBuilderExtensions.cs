using System.Reflection;

namespace IronHive.Abstractions.Pipelines;

public static class PipelineBuilderExtensions
{
    /// <summary>
    /// 파이프라인 인스턴스의 타입/특성으로 이름을 추론하여 단계로 추가합니다.
    /// </summary>
    public static IPipelineBuilder<TInput, TNext> Add<TInput, TOutput, TNext>(
        this IPipelineBuilder<TInput, TOutput> builder,
        IPipeline<TOutput, TNext> pipeline)
    {
        var type = pipeline.GetType();
        var name = type.GetCustomAttribute<PipelineNameAttribute>()?.Name
            ?? type.FullName
            ?? throw new InvalidOperationException("Pipeline must have a name");

        return builder.Add(name, pipeline);
    }

    /// <summary>
    /// 구현 타입의 특성/타입명으로 이름을 추론하여 DI로 단계 추가합니다.
    /// </summary>
    public static IPipelineBuilder<TInput, TNext> Add<TInput, TOutput, TImplementation, TNext>(
        this IPipelineBuilder<TInput, TOutput> builder,
        Func<IServiceProvider, TImplementation>? factory = null)
        where TImplementation : class, IPipeline<TOutput, TNext>
    {
        var type = typeof(TImplementation);
        var name = type.GetCustomAttribute<PipelineNameAttribute>()?.Name
            ?? type.FullName
            ?? throw new InvalidOperationException("Pipeline must have a name");

        return builder.Add<TImplementation, TNext>(name, factory);
    }

    /// <summary>
    /// 취소 토큰이 없는 비동기 델리게이트를 단계로 추가합니다.
    /// </summary>
    public static IPipelineBuilder<TInput, TNext> Add<TInput, TOutput, TNext>(
        this IPipelineBuilder<TInput, TOutput> builder,
        string name,
        Func<TOutput, Task<TNext>> function)
        => builder.Add(name, (input, ct) => function(input));

    /// <summary>
    /// 동기 델리게이트를 단계로 추가합니다.
    /// </summary>
    public static IPipelineBuilder<TInput, TNext> Add<TInput, TOutput, TNext>(
        this IPipelineBuilder<TInput, TOutput> builder,
        string name,
        Func<TOutput, TNext> function)
        => builder.Add(name, (input, ct) => Task.FromResult(function(input)));
}
