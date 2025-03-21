using IronHive.Abstractions.Memory;

namespace IronHive.Core.Memory;

public class PipelineOrchestrator : IPipelineOrchestrator
{
    private readonly IReadOnlyDictionary<string, IPipelineHandler> _handlers;
    private readonly IPipelineStorage _store;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public PipelineOrchestrator(
        IReadOnlyDictionary<string, IPipelineHandler> handlers, 
        IPipelineStorage store)
    {
        _handlers = handlers;
        _store = store;
    }

    public async Task<IEnumerable<string>> GetPipelinesAsync(
        CancellationToken cancellationToken = default)
    {
        var keys = await _store.GetKeysAsync(cancellationToken);
        return keys;
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
                    var handler = _handlers[currentStep];
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
}
