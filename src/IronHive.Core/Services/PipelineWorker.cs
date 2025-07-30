using Microsoft.Extensions.DependencyInjection;
using IronHive.Abstractions.Memory;

namespace IronHive.Core.Services;

public class PipelineWorker : IPipelineWorker
{
    private readonly IServiceProvider _services;
    private readonly PipelineWorkerConfig _config;
    private readonly IQueueStorage<PipelineRequest> _queue;
    private readonly IEnumerable<IPipelineObserver> _observers;

    private int _flag = 0;
    private CancellationTokenSource _cts = new();
    private readonly SemaphoreSlim _semaphore;

    public PipelineWorker(IServiceProvider provider, PipelineWorkerConfig config)
    {
        if (config.MaxExecutionSlots < 1)
            throw new ArgumentOutOfRangeException(nameof(config.MaxExecutionSlots));
        if (config.PollingInterval < TimeSpan.FromMilliseconds(100))
            throw new ArgumentOutOfRangeException(nameof(config.PollingInterval));
        
        _services = provider;
        _config = config;
        _queue = provider.GetRequiredService<IQueueStorage<PipelineRequest>>();
        _observers = provider.GetServices<IPipelineObserver>();
        
        _semaphore = new SemaphoreSlim(config.MaxExecutionSlots, config.MaxExecutionSlots);
    }

    /// <inheritdoc />
    public bool IsRunning
    {
        get => _flag == 1;
    }

    public void Dispose()
    {
        StopAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        _semaphore.Dispose();
        _cts.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task StartAsync()
    {
        if (Interlocked.Exchange(ref _flag, 1) == 1)
            return;

        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                // 실행 슬롯 대기
                await _semaphore.WaitAsync(_cts.Token); 
                var msg = await _queue.DequeueAsync(_cts.Token);

                // 메시지가 있으면 처리 시작
                if (msg != null)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await ExecuteAsync(msg.Message, _cts.Token);
                            if (msg.AckTag != null)
                                await _queue.AckAsync(msg.AckTag, _cts.Token);
                        }
                        finally
                        {
                            _semaphore.Release(); // 처리 후 슬롯 반환
                        }
                    }, _cts.Token).ConfigureAwait(false);
                }
                // 메시지가 없으면 일시 대기
                else
                {
                    await Task.Delay(_config.PollingInterval, _cts.Token);
                }
            }
        }
        finally
        {
            // 실행 상태 해제
             Interlocked.Exchange(ref _flag, 0);
        }
    }

    /// <inheritdoc />
    public async Task StopAsync()
    {
        if (Interlocked.CompareExchange(ref _flag, 0, 1) != 1)
            return;

        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();

        // 대기 중인 작업이 있으면 모두 취소될 때까지 대기
        while (_semaphore.CurrentCount < _config.MaxExecutionSlots)
        {
            await Task.Delay(100); // 잠시 대기 후 다시 확인
        }

        Interlocked.Exchange(ref _flag, 0);
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(PipelineRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var sourceId = request.Source.Id;
            var handlerOptions = request.HandlerOptions ?? new Dictionary<string, object?>();

            var steps = new Queue<string>(request.Steps);
            var context = new PipelineContext
            {
                Source = request.Source,
                Target = request.Target
            };
            
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
            await InvokeAllAsync(e => e.OnFailedAsync(request.Source.Id, ex));
        }
    }

    /// <summary>
    /// 이벤트를 Observer에 비동기로 전달합니다.
    /// </summary>
    private async Task InvokeAllAsync(Func<IPipelineObserver, Task> action)
    {
        await Task.WhenAll(_observers.Select(async observer =>
        {
            try
            {
                await action(observer);
            }
            catch
            {
                // 예외가 발생 무시
            }
        }));
    }
}
