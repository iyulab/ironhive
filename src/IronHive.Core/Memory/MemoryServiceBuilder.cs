using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Pipelines;
using IronHive.Abstractions.Storages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IronHive.Core.Memory;

/// <inheritdoc />
public class MemoryServiceBuilder : IMemoryServiceBuilder
{
    private readonly IServiceCollection _services;
    private IVectorStorage? _vector;
    private IQueueStorage<MemoryPipelineContext<object>>? _queue;
    private IPipelineRunner<MemoryPipelineContext<object>, MemoryPipelineContext<object>>? _pipeline;

    public MemoryServiceBuilder(IServiceCollection? services = null)
    {
        _services = services ?? new ServiceCollection();
        _services.TryAddSingleton<IMemoryService>((sp) =>
        {
            if (_queue == null)
                throw new InvalidOperationException("Queue storage is not configured.");
            if (_pipeline == null)
                throw new InvalidOperationException("Pipeline is not configured.");
            if (_vector == null)
                throw new InvalidOperationException("Vector storage is not configured.");

            var embedder = sp.GetRequiredService<IEmbeddingService>();
            return new MemoryService(_queue, _vector, embedder, _pipeline);
        });
    }

    /// <inheritdoc />
    public IMemoryServiceBuilder SetVectorStorage(IVectorStorage storage)
    {
        _vector = storage;
        return this;
    }

    /// <inheritdoc />
    public IMemoryServiceBuilder SetQueueStorage(IQueueStorage<MemoryPipelineContext<object>> storage)
    {
        _queue = storage;
        return this;
    }

    /// <inheritdoc />
    public IMemoryServiceBuilder SetPipeline()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public IMemoryService Build()
    {
        var provider = _services.BuildServiceProvider();
        return provider.GetRequiredService<IMemoryService>();
    }
}
