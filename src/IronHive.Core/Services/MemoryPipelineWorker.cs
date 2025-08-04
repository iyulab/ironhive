using Microsoft.Extensions.DependencyInjection;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Storages;

namespace IronHive.Core.Services;

/// <inheritdoc />
public class MemoryPipelineWorker : IMemoryPipelineWorker
{
    private const int DequeueInterval = 1000; // 1초

    private readonly IServiceProvider _services;
    private readonly IQueueStorage<MemoryPipelineRequest> _queue;
    private readonly IEnumerable<IMemoryPipelineObserver> _observers;

    private int _flag = 1; // 0: 시작, 1: 중지, 2: 중지 요청
    private TaskCompletionSource<bool>? _tcs = null;
    private CancellationTokenSource? _cts = null;

    public MemoryPipelineWorker(IServiceProvider services)
    {
        _services = services;
        _queue = services.GetRequiredService<IQueueStorage<MemoryPipelineRequest>>();
        _observers = services.GetServices<IMemoryPipelineObserver>();
    }

    /// <inheritdoc />
    public PipelineWorkerStatus Status => _flag switch
    {
        0 => PipelineWorkerStatus.Started,
        1 => PipelineWorkerStatus.Stopped,
        2 => PipelineWorkerStatus.StopRequested,
        _ => throw new InvalidOperationException("Invalid worker status")
    };

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        _tcs?.TrySetCanceled();
        _tcs = null;

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task StartAsync()
    {
        // 중지 상태가 아닐 경우 아무 작업도 하지 않음
        if (Interlocked.CompareExchange(ref _flag, 0, 1) != 1)
            return;

        _cts = new CancellationTokenSource();
        _tcs = new TaskCompletionSource<bool>();

        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var msg = await _queue.DequeueAsync(_cts.Token);
                // 메시지가 있으면 처리 시작
                if (msg != null)
                {
                    await ExecuteAsync(msg.Message, _cts.Token);
                    if (msg.AckTag != null)
                        await _queue.AckAsync(msg.AckTag, _cts.Token);
                }
                // 메시지가 없으면 일시 대기
                else
                {
                    await Task.Delay(Math.Max(DequeueInterval, 100), _cts.Token);
                }

                // 중지 요청이 들어온 경우
                if (_flag == 2) break;
            }
        }
        finally
        {
            _cts?.Dispose();
            _tcs?.TrySetResult(true);
            _flag = 1; // 작업이 중지됨
        }
    }

    /// <inheritdoc />
    public async Task StopAsync(bool force = false)
    {
        // 즉시 중지 요청인 경우
        if (force)
        {
            _cts?.Cancel();
        }
        // 즉시 중지가 아닌 경우,
        else
        {
            Interlocked.CompareExchange(ref _flag, 2, 0);
        }

        // 작업이 완료될 때까지 대기
        var tcs = _tcs;
        if (tcs != null)
        {
            await tcs.Task;
        }
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(MemoryPipelineRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var sourceId = request.Source.Id;
            var options = request.HandlerOptions ?? new Dictionary<string, object?>();

            var steps = new Queue<string>(request.Steps);
            var context = new MemoryPipelineContext
            {
                Source = request.Source,
                Target = request.Target,
            };
            
            await NotifyObserversAsync(e => e.OnStartedAsync(sourceId));

            while (!cancellationToken.IsCancellationRequested && steps.TryDequeue(out var step))
            {
                if (string.IsNullOrWhiteSpace(step))
                    continue;

#pragma warning disable IDE0063 // 간단한 using 문을 사용하지 않습니다.
                using (var scope = _services.CreateScope())
                {
                    var handler = scope.ServiceProvider.GetRequiredKeyedService<IMemoryPipelineHandler>(step);
                    context.Options = options.TryGetValue(step, out var option) ? option : null;

                    await NotifyObserversAsync(e => e.OnProcessBeforeAsync(sourceId, step, context));
                    context = await handler.ProcessAsync(context, cancellationToken);
                    await NotifyObserversAsync(e => e.OnProcessAfterAsync(sourceId, step, context));
                }
#pragma warning restore IDE0063
            }

            await NotifyObserversAsync(e => e.OnCompletedAsync(sourceId));
        }
        catch (Exception ex)
        {
            await NotifyObserversAsync(e => e.OnFailedAsync(request.Source.Id, ex));
        }
    }

    /// <summary>
    /// 이벤트를 Observer에 비동기로 전달합니다.
    /// </summary>
    private async Task NotifyObserversAsync(Func<IMemoryPipelineObserver, Task> action)
    {
        await Task.WhenAll(_observers.Select(async observer =>
        {
            try
            {
                await action(observer);
            }
            catch
            {
                // 예외 발생 무시
            }
        }));
    }
}
