using Microsoft.Extensions.DependencyInjection;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Storages;

namespace IronHive.Core.Memory;

/// <inheritdoc />
public class MemoryWorker : IMemoryWorker
{
    private const int DequeueInterval = 1000; // 1초

    private readonly IServiceProvider _services;
    private readonly IQueueStorage<MemoryPipelineRequest> _queue;

    private int _state = (int)MemoryWorkerState.Stopped;
    private TaskCompletionSource<bool>? _tcs = null;
    private CancellationTokenSource? _cts = null;

    public MemoryWorker(IServiceProvider services)
    {
        _services = services;
        _queue = services.GetRequiredService<IQueueStorage<MemoryPipelineRequest>>();
    }

    /// <inheritdoc />
    public event EventHandler<MemoryWorkerState>? StateChanged;

    /// <inheritdoc />
    public event EventHandler<MemoryPipelineEventArgs>? Progressed;

    /// <inheritdoc />
    public MemoryWorkerState State
    {
        get => (MemoryWorkerState)Volatile.Read(ref _state);
        private set
        {
            var prev = Interlocked.Exchange(ref _state, (int)value);

            // 상태가 변경되었을 때만 이벤트를 발생시킴
            if (prev != (int)value)
            {
                StateChanged?.Invoke(this, value);
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        _tcs?.TrySetCanceled();
        _tcs = null;

        StateChanged = null;
        Progressed = null;

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task StartAsync()
    {
        // 중지 상태가 아닌 경우, 아무 작업도 하지 않음
        if (Interlocked.CompareExchange(ref _state, (int)MemoryWorkerState.StartRequested, (int)MemoryWorkerState.Stopped)
            != (int)MemoryWorkerState.Stopped)
            return;

        // 최초 변경 수동 호출 (CompareExchange를 사용했기 때문)
        StateChanged?.Invoke(this, State); 

        _cts = new CancellationTokenSource();
        _tcs = new TaskCompletionSource<bool>();

        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var msg = await _queue.DequeueAsync(_cts.Token);

                if (State == MemoryWorkerState.StopRequested)
                    break;

                if (msg != null)
                {
                    State = MemoryWorkerState.Processing;
                    await ExecuteAsync(msg.Payload, _cts.Token);
                    if (msg.Tag != null)
                        await _queue.AckAsync(msg.Tag, _cts.Token);
                }
                else
                {
                    State = MemoryWorkerState.Idle;
                    await Task.Delay(Math.Max(DequeueInterval, 100), _cts.Token);
                }
            }
        }
        finally
        {
            _cts?.Dispose();
            _tcs?.TrySetResult(true);
            State = MemoryWorkerState.Stopped;
        }
    }

    /// <inheritdoc />
    public async Task StopAsync(bool force = false)
    {
        // 즉시 중지 요청인 경우
        if (force)
        {
            _cts?.Cancel();
            State = MemoryWorkerState.Stopped;
        }
        // 즉시 중지가 아닌 경우
        else
        {
            State = MemoryWorkerState.StopRequested;
        }

        // 작업이 완료될 때까지 대기
        var tcs = _tcs;
        if (tcs != null)
        {
            await tcs.Task;
        }
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(
        MemoryPipelineRequest request, 
        CancellationToken cancellationToken = default)
    {
        var sourceId = request.Source.Id;
        var options = request.HandlerOptions ?? new Dictionary<string, object?>();
        var context = new MemoryPipelineContext
        {
            Source = request.Source,
            Target = request.Target,
        };
        var steps = new Queue<string>(request.Steps);

        try
        {
            Progressed?.Invoke(this, new MemoryPipelineEventArgs(PipelineStatus.Started, context));

            while (!cancellationToken.IsCancellationRequested && steps.TryDequeue(out var step))
            {
                if (string.IsNullOrWhiteSpace(step))
                    continue;

                context.CurrentStep = step;
                context.Options = options.TryGetValue(step, out var option) ? option : null;

#pragma warning disable IDE0063 // 간단한 using 문을 사용하지 않습니다.
                using (var scope = _services.CreateScope())
                {
                    var handler = scope.ServiceProvider.GetRequiredKeyedService<IMemoryPipelineHandler>(step);
                    
                    Progressed?.Invoke(this, new MemoryPipelineEventArgs(PipelineStatus.ProcessBefore, context));
                    context = await handler.ProcessAsync(context, cancellationToken);
                    Progressed?.Invoke(this, new MemoryPipelineEventArgs(PipelineStatus.ProcessAfter, context));
                }
#pragma warning restore IDE0063
            }

            Progressed?.Invoke(this, new MemoryPipelineEventArgs(PipelineStatus.Completed, context));
        }
        catch (Exception ex)
        {
            Progressed?.Invoke(this, new MemoryPipelineErrorEventArgs(context, ex));
        }
    }
}
