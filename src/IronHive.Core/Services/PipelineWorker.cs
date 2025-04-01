using IronHive.Abstractions.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core.Services;

public class PipelineWorker : IPipelineWorker
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IServiceProvider _services;

    private readonly IPipelineStorage _store;
    private readonly IQueueStorage _queue;

    public bool IsWorking 
    {
        get => _semaphore.CurrentCount == 0;
    }

    public required string QueueName { get; init; }

    public PipelineWorker(IServiceProvider provider)
    {
        _services = provider;
        _store = provider.GetRequiredService<IPipelineStorage>();
        _queue = provider.GetRequiredService<IQueueStorage>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (IsWorking)
        {
            return;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            var id = await _queue.DequeueAsync<string>(QueueName, cancellationToken);
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

    public async Task RunPipelineAsync(
        DataPipeline pipeline,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            pipeline = pipeline.Start();
            await _store.SetAsync(pipeline, cancellationToken);

            while (pipeline.Status == PipelineStatus.Processing)
            {
                var currentStep = pipeline.CurrentStep;
                if (currentStep == null)
                {
                    pipeline = pipeline.Complete();
                    await _store.SetAsync(pipeline, cancellationToken);
                    break;
                }
                else
                {
                    var handler = GetHandler(currentStep);
                    pipeline = await handler.ProcessAsync(pipeline, cancellationToken);
                    pipeline = pipeline.Next();
                    await _store.SetAsync(pipeline, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            pipeline = pipeline.Failed(ex.Message);
            await _store.SetAsync(pipeline, cancellationToken);
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
