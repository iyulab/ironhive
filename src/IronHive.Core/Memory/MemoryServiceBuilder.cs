using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Pipelines;
using IronHive.Abstractions.Queue;
using IronHive.Abstractions.Registries;
using IronHive.Abstractions.Vector;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core.Memory;

/// <inheritdoc />
public class MemoryServiceBuilder : IMemoryServiceBuilder
{
    private readonly IServiceProvider _services;

    private IMemoryTarget? _memoryTarget;
    private MemoryWorkerConfig? _workerConfig;
    private PipelineBuildDelegate? _factory;

    public MemoryServiceBuilder(IServiceProvider services)
    {
        _services = services;
    }

    /// <inheritdoc />
    public IMemoryServiceBuilder Use(IMemoryTarget target)
    {
        _memoryTarget = target;
        return this;
    }

    /// <inheritdoc />
    public IMemoryServiceBuilder SetWorker(MemoryWorkerConfig config)
    {
        _workerConfig = config;
        return this;
    }

    /// <inheritdoc />
    public IMemoryServiceBuilder SetPipeline(PipelineBuildDelegate configure)
    {
        _factory = configure;
        return this;
    }

    /// <inheritdoc />
    public IMemoryService Build()
    {
        if (_factory == null)
            throw new InvalidOperationException("Pipeline is not configured. Please call SetPipeline method to configure it.");
        if (_memoryTarget == null)
            throw new InvalidOperationException("Memory target is not configured. Please call UseStorage method to configure it.");
        if (_workerConfig == null)
            throw new InvalidOperationException("Memory Worker is not configu");

        var embedder = _services.GetRequiredService<IEmbeddingService>();
        var pipeline = _factory(PipelineFactory.Create<PipelineContext>(_services));

        var storages = _services.GetRequiredService<IStorageRegistry>();
        if (!storages.TryGet<IQueueStorage>(_workerConfig.QueueName, out var queue))
            throw new InvalidOperationException($"Queue storage '{_workerConfig.QueueName}' is not registered.");

        if (_memoryTarget is VectorMemoryTarget vectorTarget)
        {
            if (!storages.TryGet<IVectorStorage>(vectorTarget.StorageName, out var vector))
                throw new InvalidOperationException($"Vector storage '{vectorTarget.StorageName}' is not registered.");

            return new VectorMemoryService(queue, vector, embedder, pipeline)
            {
                StorageName = vectorTarget.StorageName,
                CollectionName = vectorTarget.CollectionName,
                Workers = new MemoryWorkerService(queue, pipeline)
                {
                    MinCount = _workerConfig.MinWorkerCount,
                    MaxCount = _workerConfig.MaxWorkerCount,
                    DequeueInterval = TimeSpan.FromMilliseconds(_workerConfig.DequeueInterval),
                },
            };
        }
        else
        {
            throw new NotSupportedException($"Memory target of type '{_memoryTarget.GetType().Name}' is not supported.");
        }
    }
}
