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
    private readonly List<object> _items = [];
    private PipelineBuildDelegate? _factory;

    public MemoryServiceBuilder(IServiceCollection? services = null)
    {
        _services = services ?? new ServiceCollection();
        _services.TryAddSingleton<IMemoryService>((sp) =>
        {
            var queues = _items.OfType<IQueueStorage>().ToList();
            var vectors = _items.OfType<IVectorStorage>().ToList();
            var embedder = sp.GetRequiredService<IEmbeddingService>();
            if (_factory == null)
                throw new InvalidOperationException("Pipeline is not configured. Please call SetPipeline method to configure it.");
            var pipeline = _factory(PipelineFactory.Create<PipelineContext>(sp));

            return new MemoryService(sp, embedder, queues, vectors, pipeline);
        });
    }

    /// <inheritdoc />
    public IMemoryServiceBuilder AddVectorStorage(IVectorStorage storage)
    {
        _items.Add(storage);
        return this;
    }

    /// <inheritdoc />
    public IMemoryServiceBuilder AddQueueStorage(IQueueStorage storage)
    {
        _items.Add(storage);
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
        var provider = _services.BuildServiceProvider();
        return provider.GetRequiredService<IMemoryService>();
    }
}
