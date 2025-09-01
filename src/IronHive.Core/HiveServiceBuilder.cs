using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Tools;
using IronHive.Abstractions;
using IronHive.Abstractions.Files;
using IronHive.Core.Services;
using IronHive.Abstractions.Catalog;
using IronHive.Core.Files;
using IronHive.Core.Memory;
using IronHive.Abstractions.Messages;

namespace IronHive.Core;

/// <inheritdoc />
public class HiveServiceBuilder : IHiveServiceBuilder
{
    /// <summary>
    /// 임시 서비스 저장소
    /// </summary>
    private readonly List<object> _items = [];

    public HiveServiceBuilder(IServiceCollection? services = null)
    {
        Services = services ?? new ServiceCollection();

        // AI 서비스
        Services.TryAddSingleton<IModelCatalogService>(sp =>
        {
            var providers = _items.OfType<IModelCatalogProvider>();
            return new ModelCatalogService(providers);
        });
        Services.TryAddSingleton<IMessageService>(sp =>
        {
            var generators = _items.OfType<IMessageGenerator>();
            var tools = _items.OfType<ITool>();
            return new MessageService(sp, generators, tools);
        });
        Services.TryAddSingleton<IEmbeddingService>(sp =>
        {
            var generators = _items.OfType<IEmbeddingGenerator>();
            return new EmbeddingService(generators);
        });

        Files = new FileServiceBuilder(Services);
        Memory = new MemoryServiceBuilder(Services);
    }

    /// <inheritdoc />
    public IServiceCollection Services { get; }

    /// <inheritdoc />
    public IFileServiceBuilder Files { get; }

    /// <inheritdoc />
    public IMemoryServiceBuilder Memory { get; }

    /// <inheritdoc />
    public IHiveServiceBuilder AddModelCatalogProvider(IModelCatalogProvider provider)
    {
        _items.Add(provider);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddEmbeddingGenerator(IEmbeddingGenerator generator)
    {
        _items.Add(generator);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddMessageGenerator(IMessageGenerator generator)
    {
        _items.Add(generator);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddMessageTool(ITool tool)
    {
        _items.Add(tool);
        return this;
    }

    /// <inheritdoc />
    public IHiveService Build()
    {
        var provider = Services.BuildServiceProvider();
        return new HiveService(provider);
    }
}
