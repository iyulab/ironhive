using IronHive.Abstractions.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core.Memory;

public class PipelineOrchestrator : IPipelineOrchestrator
{
    private readonly IServiceProvider _services;
    private readonly IPipelineStorage _store;
    public SemaphoreSlim _semaphore = new(1, 1);

    public PipelineOrchestrator(IServiceProvider provider, IPipelineStorage store)
    {
        _services = provider;
        _store = store;
    }

    public async Task<IEnumerable<string>> GetPipelinesAsync(
        CancellationToken cancellationToken = default)
    {
        var keys = await _store.GetKeysAsync(cancellationToken);
        return keys;
    }

    public async Task<DataPipeline> GetPipelineAsync(
        string pipelineId,
        CancellationToken cancellationToken = default)
    {
        var pipeline = await _store.GetValueAsync<DataPipeline>(pipelineId, cancellationToken);
        return pipeline;
    }

    public async Task RunPipelineAsync(
        DataPipeline pipeline,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            pipeline = pipeline.Start();
            await _store.SetValueAsync(pipeline.Id, pipeline, cancellationToken);

            while (pipeline.Status == PipelineStatus.Processing)
            {
                var currentStep = pipeline.CurrentStep;
                if (currentStep == null)
                {
                    pipeline = pipeline.Complete();
                    await _store.SetValueAsync(pipeline.Id, pipeline, cancellationToken);
                    break;
                }
                else
                {
                    var handler = GetHandler(currentStep);
                    pipeline = await handler.ProcessAsync(pipeline, cancellationToken);
                    pipeline = pipeline.Next();
                    await _store.SetValueAsync(pipeline.Id, pipeline, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            pipeline = pipeline.Failed(ex.Message);
            await _store.SetValueAsync(pipeline.Id, pipeline, cancellationToken);
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
