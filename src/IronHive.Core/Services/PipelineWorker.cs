using IronHive.Abstractions.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core.Services;

public class PipelineWorker : IPipelineWorker
{
    private readonly IServiceProvider _services;
    private readonly IPipelineStorage _store;
    private readonly IQueueStorage _queue;
    private readonly IPipelineEventHandler? _event;

    private SemaphoreSlim _semaphore;

    public required int MaxConcurrent { get; init; } = 3;

    public bool IsWorking
    {
        get => _semaphore.CurrentCount == 0;
    }

    public PipelineWorker(IServiceProvider provider)
    {
        _services = provider;
        _store = provider.GetRequiredService<IPipelineStorage>();
        _queue = provider.GetRequiredService<IQueueStorage>();

        // TODO: NULLABLE??????
        _event = provider.GetService<IPipelineEventHandler>();

        if (MaxConcurrent < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxConcurrent));
        }

        _semaphore = new SemaphoreSlim(MaxConcurrent, MaxConcurrent);
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (IsWorking)
        {
            return;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            var id = await _queue.DequeueAsync(cancellationToken);
            if (string.IsNullOrEmpty(id))
            {
                await Task.Delay(1_000, cancellationToken);
                continue;
            }
            else
            {
                var pipeline = await _store.GetAsync(id, cancellationToken);
                await RunPipelineAsync(pipeline, cancellationToken);
            }
        }
    }

    /// <inheritdoc />
    public async Task RunPipelineAsync(
        PipelineContext pipeline,
        CancellationToken cancellationToken = default)
    {
        // TODO: Context와 Pipeline 상태 변경을 분리
        // ?? 객체안에 Context를 넣어서 상태 변경을 분리
        // ?? 객체는 Worker에서만 사용하고, Context는 Handler에서만 사용
        // ?? 객체 생성 요구, 네이밍 모름

        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            pipeline = pipeline.Start();
            await _store.SetAsync(pipeline, cancellationToken);
            _event?.OnStartedAsync(pipeline);

            while (pipeline.Status == PipelineStatus.Processing)
            {
                var currentStep = pipeline.CurrentStep;
                if (currentStep == null)
                {
                    pipeline = pipeline.Complete();
                    await _store.DeleteAsync(pipeline.Id, cancellationToken);
                    _event?.OnCompletedAsync(pipeline);
                    break;
                }
                else
                {
                    var handler = GetHandler(currentStep);
                    pipeline = await handler.ProcessAsync(pipeline, cancellationToken);
                    pipeline = pipeline.Next();
                    await _store.SetAsync(pipeline, cancellationToken);
                    _event?.OnStepedAsync(pipeline);
                }
            }
        }
        catch (Exception ex)
        {
            pipeline = pipeline.Failed(ex.Message);
            await _store.DeleteAsync(pipeline.Id, cancellationToken);
            _event?.OnFailedAsync(pipeline, ex);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private IPipelineHandler GetHandler(string serviceKey)
    {
        using var scope = _services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredKeyedService<IPipelineHandler>(serviceKey);
        return handler;
    }
}
