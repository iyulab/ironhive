namespace IronHive.Abstractions.Pipelines;

/// <summary>
/// 순서를 보장하는 파이프라인의 단일 스텝을 나타냅니다.
/// </summary>
/// <param name="Name">스텝 고유 이름입니다.</param>
/// <param name="Pipeline">실행할 파이프라인 구현입니다.</param>
public sealed record PipelineStep(string Name, IPipeline<object?, object?> Pipeline);

/// <summary>
/// 순서를 보장하는 파이프라인의 마지막 스텝을 나타냅니다.
/// </summary>
/// <param name="Name">스텝 고유 이름입니다.</param>
/// <param name="Pipeline">실행할 파이프라인 구현입니다.</param>
public sealed record PipelineFinalStep(string Name, IPipeline<object?> Pipeline);

/// <inheritdoc />
public class PipelineRunner<TInput, TOutput> : IPipelineRunner<TInput, TOutput>
{
    private readonly IReadOnlyList<PipelineStep> _steps;
    private readonly IPipelineHook? _hook;

    public PipelineRunner(IReadOnlyList<PipelineStep> steps, IPipelineHook? hook = null)
    {
        _steps = steps ?? throw new ArgumentNullException(nameof(steps));
        if (_steps.Count == 0)
            throw new ArgumentException("적어도 하나의 파이프라인 스텝이 필요합니다.", nameof(steps));

        _hook = hook;
    }

    /// <inheritdoc />
    public Task<TOutput> InvokeAsync(TInput input, CancellationToken cancellationToken = default)
        => InvokeFromAsync(_steps[0].Name, input, cancellationToken);

    /// <inheritdoc />
    public async Task<TOutput> InvokeFromAsync(
        string name, 
        TInput input, 
        CancellationToken cancellationToken = default)
    {
        var idx = FindIndexOf(name);
        object? current = input;
        for (int i = idx; i < _steps.Count; i++)
        {
            var step = _steps[i];
            try
            {
                await InvokeHookSafely(h => h.BeforeAsync(step.Name, current, cancellationToken));
                current = await step.Pipeline.InvokeAsync(current, cancellationToken);
                await InvokeHookSafely(h => h.AfterAsync(step.Name, current, cancellationToken));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                await InvokeHookSafely(h => h.CancelAsync(step.Name, cancellationToken));
                throw;
            }
            catch (Exception ex)
            {
                await InvokeHookSafely(h => h.ErrorAsync(step.Name, ex, cancellationToken));
                throw;
            }
        }
        return (TOutput)current!;
    }

    /// <summary>
    /// 지정한 이름을 가진 스텝의 인덱스를 반환합니다.
    /// </summary>
    private int FindIndexOf(string name)
    {
        for (int i = 0; i < _steps.Count; i++)
            if (string.Equals(_steps[i].Name, name, StringComparison.Ordinal)) return i;

        throw new KeyNotFoundException($"Pipeline with name '{name}' not found");
    }

    /// <summary>
    /// 훅 실행 중 발생한 예외는 파이프라인 흐름에 영향을 주지 않도록 안전하게 무시하며 호출합니다.
    /// </summary>
    private async ValueTask InvokeHookSafely(Func<IPipelineHook, ValueTask> action)
    {
        if (_hook is null) return;
        try { await action(_hook).ConfigureAwait(false); }
        catch { /* 훅은 파이프라인 흐름을 방해하지 않도록 무시 */ }
    }
}

/// <inheritdoc />
public class PipelineRunner<TInput> : IPipelineRunner<TInput>
{
    private readonly IReadOnlyList<PipelineStep> _steps;
    private readonly PipelineFinalStep _finalStep;
    private readonly IPipelineHook? _hook;

    public PipelineRunner(
        IReadOnlyList<PipelineStep> steps,
        PipelineFinalStep finalStep,
        IPipelineHook? hook = null)
    {
        _steps = steps ?? throw new ArgumentNullException(nameof(steps));
        if (_steps.Count == 0)
            throw new ArgumentException("적어도 하나의 파이프라인 스텝이 필요합니다.", nameof(steps));
        _finalStep = finalStep;
        _hook = hook;
    }

    /// <inheritdoc />
    public Task InvokeAsync(TInput input, CancellationToken cancellationToken = default)
        => InvokeFromAsync(_steps[0].Name, input, cancellationToken);

    /// <inheritdoc />
    public async Task InvokeFromAsync(
        string name,
        TInput input,
        CancellationToken cancellationToken = default)
    {
        var idx = FindIndexOf(name);
        object? current = input;
        for (int i = idx; i < _steps.Count; i++)
        {
            var step = _steps[i];
            try
            {
                await InvokeHookSafely(h => h.BeforeAsync(step.Name, current, cancellationToken));
                current = await step.Pipeline.InvokeAsync(current, cancellationToken);
                await InvokeHookSafely(h => h.AfterAsync(step.Name, current, cancellationToken));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                await InvokeHookSafely(h => h.CancelAsync(step.Name, cancellationToken));
                throw;
            }
            catch (Exception ex)
            {
                await InvokeHookSafely(h => h.ErrorAsync(step.Name, ex, cancellationToken));
                throw;
            }
        }

        try
        {
            await InvokeHookSafely(h => h.BeforeAsync(_finalStep.Name, current, cancellationToken));
            await _finalStep.Pipeline.InvokeAsync(current, cancellationToken);
            await InvokeHookSafely(h => h.AfterAsync(_finalStep.Name, null, cancellationToken));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await InvokeHookSafely(h => h.CancelAsync(_finalStep.Name, cancellationToken));
            throw;
        }
        catch (Exception ex)
        {
            await InvokeHookSafely(h => h.ErrorAsync(_finalStep.Name, ex, cancellationToken));
            throw;
        }
    }

    /// <summary>
    /// 지정한 이름을 가진 스텝의 인덱스를 반환합니다.
    /// </summary>
    private int FindIndexOf(string name)
    {
        for (int i = 0; i < _steps.Count; i++)
            if (string.Equals(_steps[i].Name, name, StringComparison.Ordinal)) return i;

        throw new KeyNotFoundException($"Pipeline with name '{name}' not found");
    }

    /// <summary>
    /// 훅 실행 중 발생한 예외는 파이프라인 흐름에 영향을 주지 않도록 안전하게 무시하며 호출합니다.
    /// </summary>
    private async ValueTask InvokeHookSafely(Func<IPipelineHook, ValueTask> action)
    {
        if (_hook is null) return;
        try { await action(_hook).ConfigureAwait(false); }
        catch { /* 훅은 파이프라인 흐름을 방해하지 않도록 무시 */ }
    }
}
