using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// IAgent를 ITypedExecutor로 래핑합니다.
/// </summary>
public class AgentExecutor<TInput, TOutput> : ITypedExecutor<TInput, TOutput>
{
    private readonly IAgent _agent;
    private readonly Func<TInput, IEnumerable<Message>> _inputConverter;
    private readonly Func<MessageResponse, TOutput> _outputConverter;

    /// <inheritdoc />
    public string Name => _agent.Name;

    public AgentExecutor(
        IAgent agent,
        Func<TInput, IEnumerable<Message>> inputConverter,
        Func<MessageResponse, TOutput> outputConverter)
    {
        _agent = agent ?? throw new ArgumentNullException(nameof(agent));
        _inputConverter = inputConverter ?? throw new ArgumentNullException(nameof(inputConverter));
        _outputConverter = outputConverter ?? throw new ArgumentNullException(nameof(outputConverter));
    }

    /// <inheritdoc />
    public async Task<TOutput> ExecuteAsync(TInput input, CancellationToken ct = default)
    {
        var messages = _inputConverter(input);
        var response = await _agent.InvokeAsync(messages, ct).ConfigureAwait(false);
        return _outputConverter(response);
    }
}

/// <summary>
/// 타입 안전 파이프라인을 구성하는 빌더 진입점입니다.
/// </summary>
public static class TypedPipeline
{
    /// <summary>
    /// 첫 번째 Executor로 파이프라인을 시작합니다.
    /// </summary>
    public static TypedPipelineBuilder<TInput, TOutput> Start<TInput, TOutput>(
        ITypedExecutor<TInput, TOutput> first)
    {
        return new TypedPipelineBuilder<TInput, TOutput>(first);
    }
}

/// <summary>
/// 타입 안전 파이프라인을 단계별로 구성하는 빌더입니다.
/// </summary>
public class TypedPipelineBuilder<TFirst, TCurrent>
{
    private readonly ITypedExecutor<TFirst, TCurrent> _executor;

    internal TypedPipelineBuilder(ITypedExecutor<TFirst, TCurrent> executor)
    {
        _executor = executor;
    }

    /// <summary>
    /// 다음 단계를 파이프라인에 추가합니다.
    /// </summary>
    public TypedPipelineBuilder<TFirst, TNext> Then<TNext>(
        ITypedExecutor<TCurrent, TNext> next)
    {
        var chained = new ChainedExecutor<TFirst, TCurrent, TNext>(_executor, next);
        return new TypedPipelineBuilder<TFirst, TNext>(chained);
    }

    /// <summary>
    /// 파이프라인을 빌드하여 단일 Executor로 반환합니다.
    /// </summary>
    public ITypedExecutor<TFirst, TCurrent> Build() => _executor;
}

/// <summary>
/// 두 Executor를 체이닝하는 내부 구현입니다.
/// </summary>
internal sealed class ChainedExecutor<TInput, TMiddle, TOutput> : ITypedExecutor<TInput, TOutput>
{
    private readonly ITypedExecutor<TInput, TMiddle> _first;
    private readonly ITypedExecutor<TMiddle, TOutput> _second;

    public string Name => $"{_first.Name} -> {_second.Name}";

    internal ChainedExecutor(
        ITypedExecutor<TInput, TMiddle> first,
        ITypedExecutor<TMiddle, TOutput> second)
    {
        _first = first;
        _second = second;
    }

    public async Task<TOutput> ExecuteAsync(TInput input, CancellationToken ct = default)
    {
        var middle = await _first.ExecuteAsync(input, ct).ConfigureAwait(false);
        return await _second.ExecuteAsync(middle, ct).ConfigureAwait(false);
    }
}
