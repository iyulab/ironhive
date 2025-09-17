using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Queue;
using IronHive.Abstractions.Workflow;

namespace IronHive.Core.Memory;

/// <inheritdoc />
public class MemoryWorker : IMemoryWorker
{
    private readonly IQueueStorage _queue;
    private readonly IWorkflow<MemoryContext> _pipeline;

    private int _state = (int)MemoryWorkerState.Stopped;
    private TaskCompletionSource<bool>? _tcs = null;
    private CancellationTokenSource? _cts = null;

    public MemoryWorker(IQueueStorage queue, IWorkflow<MemoryContext> pipeline)
    {
        _queue = queue;
        _pipeline = pipeline;
    }

    /// <inheritdoc />
    public required TimeSpan DequeueInterval { get; init; }

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
    public event EventHandler<MemoryWorkerState>? StateChanged;

    /// <inheritdoc />
    public void Dispose()
    {
        try
        {
            _cts?.Dispose();
            _tcs?.TrySetCanceled();
        }
        finally
        {
            _cts = null;
            _tcs = null;

            StateChanged = null;
        }
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
                if (State == MemoryWorkerState.StopRequested)
                    break;

                var msg = await _queue.DequeueAsync<MemoryContext>(_cts.Token);
                if (msg != null)
                {
                    State = MemoryWorkerState.Processing;
                    await _pipeline.RunAsync(msg.Body, _cts.Token);
                    await msg.CompleteAsync(_cts.Token);
                }
                else
                {
                    State = MemoryWorkerState.Idle;
                    await Task.Delay(DequeueInterval, _cts.Token);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Worker encountered an error: {ex.Message}");
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
}
