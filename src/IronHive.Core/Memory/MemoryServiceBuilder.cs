using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IronHive.Core.Memory;

/// <inheritdoc />
public class MemoryServiceBuilder : IMemoryServiceBuilder
{
    private readonly IServiceCollection _services;
    private PipelineBuildDelegate? _factory;

    public MemoryServiceBuilder(IServiceCollection? services = null)
    {
        _services = services ?? new ServiceCollection();
        _services.TryAddSingleton<IMemoryService>((sp) =>
        {
            if (_factory == null)
                throw new InvalidOperationException("Pipeline is not configured. Please call SetPipeline method to configure it.");
            var pipeline = _factory(PipelineFactory.Create<PipelineContext>(sp));

            return new MemoryService(sp, pipeline);
        });
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
