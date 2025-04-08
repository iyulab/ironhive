using IronHive.Abstractions.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core.Services;

public class PipelineWorker : IPipelineWorker
{
    private readonly IServiceProvider _services;
    private readonly IQueueStorage _queue;
    private readonly IEnumerable<IPipelineObserver> _events;

    private int _runningFlag = 0;
    private readonly SemaphoreSlim _semaphore;

    public bool IsActive => _runningFlag == 1;
    public int AvailableExecutionSlots => _semaphore.CurrentCount;

    public PipelineWorker(IServiceProvider provider, PipelineWorkerConfig config)
    {
        MaxExecutionSlots = config.MaxExecutionSlots;
        PollingInterval = config.PollingInterval;

        if (MaxExecutionSlots < 1)
            throw new ArgumentOutOfRangeException(nameof(MaxExecutionSlots));
        if (PollingInterval < TimeSpan.FromMilliseconds(100))
            throw new ArgumentOutOfRangeException(nameof(PollingInterval));
        
        _services = provider;
        _queue = provider.GetRequiredService<IQueueStorage>();
        _events = provider.GetServices<IPipelineObserver>();
        _semaphore = new SemaphoreSlim(MaxExecutionSlots, MaxExecutionSlots);
    }

    public int MaxExecutionSlots { get; }

    public TimeSpan PollingInterval { get; }

    public void Dispose()
    {
        _semaphore.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _runningFlag, 1) == 1)
            return;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // 실행 가능한 슬롯이 없으면 PollingInterval 만큼 대기
                if (_semaphore.CurrentCount == 0)
                {
                    await Task.Delay(PollingInterval, cancellationToken);
                    continue;
                }

                var result = await _queue.DequeueAsync<PipelineRequest>(cancellationToken);
                if (result == null)
                {
                    // 폴링 말고 구독 이벤트 방법 확인
                    await Task.Delay(PollingInterval, cancellationToken);
                    continue;
                }

                _ = Task.Run(async () =>
                {
                    await ExecuteAsync(result.Message, cancellationToken);
                    if (result.AckTag != null)
                    {
                        // 처리 확인 아웃
                        await _queue.AckAsync(result.AckTag, cancellationToken);
                    }
                }, cancellationToken);
            }
        }
        finally
        {
            // 필요하면 실행 상태 해제 가능 (ex. 재시작 허용 시)
             Interlocked.Exchange(ref _runningFlag, 0);
        }
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(PipelineRequest request, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        var sourceId = request.Source.Id;
        var steps = new Queue<string>(request.Steps);
        var context = new PipelineContext
        {
            Source = request.Source,
            Target = request.Target
        };
        var handlerOptions = request.HandlerOptions ?? new Dictionary<string, object?>();

        try
        {
            await InvokeAllAsync(e => e.OnStartedAsync(sourceId));

            while (!cancellationToken.IsCancellationRequested && steps.TryDequeue(out var step))
            {
                if (string.IsNullOrWhiteSpace(step))
                    continue;

                using var scope = _services.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredKeyedService<IPipelineHandler>(step);

                if (handlerOptions.TryGetValue(step, out var options))
                {
                    context.Options = options;
                }

                await InvokeAllAsync(e => e.OnProcessBeforeAsync(sourceId, step, context));

                context = await handler.ProcessAsync(context, cancellationToken);

                await InvokeAllAsync(e => e.OnProcessAfterAsync(sourceId, step, context));
            }

            await InvokeAllAsync(e => e.OnCompletedAsync(sourceId));
        }
        catch (Exception ex)
        {
            await InvokeAllAsync(e => e.OnFailedAsync(sourceId, ex));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // 이벤트들을 모두 인보크 해주는 메서드
    private Task InvokeAllAsync(Func<IPipelineObserver, Task> action)
    {
        return Task.WhenAll(_events.Select(e => action(e)));
    }
}
