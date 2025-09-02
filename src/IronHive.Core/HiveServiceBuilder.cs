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
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Queue;
using IronHive.Abstractions.Vector;
using IronHive.Core.Agent;

namespace IronHive.Core;

/// <inheritdoc />
public class HiveServiceBuilder : IHiveServiceBuilder
{
    private readonly IKeyedCollectionGroup<IKeyedProvider> _providers;
    private readonly IKeyedCollectionGroup<IKeyedStorage> _storages;

    public HiveServiceBuilder(IServiceCollection? services = null)
    {
        Services = services ?? new ServiceCollection();

        // 등록된 Provider 및 Storage 관리
        _providers = new KeyedCollectionGroup<IKeyedProvider>(p => p.ProviderName);
        _storages = new KeyedCollectionGroup<IKeyedStorage>(s => s.StorageName);
        Services.AddSingleton(_providers);
        Services.AddSingleton(_storages);

        // AI 서비스
        Services.TryAddSingleton<IModelCatalogService>();
        Services.TryAddSingleton<IEmbeddingService>();

        // 서비스 빌더
        Agents = new AgentServiceBuilder(Services);
        Files = new FileServiceBuilder(Services);
        Memory = new MemoryServiceBuilder(Services);
    }

    /// <inheritdoc />
    public IServiceCollection Services { get; }

    /// <inheritdoc />
    public IAgentServiceBuilder Agents { get; }

    /// <inheritdoc />
    public IFileServiceBuilder Files { get; }

    /// <inheritdoc />
    public IMemoryServiceBuilder Memory { get; }

    /// <inheritdoc />
    public IHiveServiceBuilder AddModelCatalogProvider(IModelCatalogProvider provider)
    {
        _providers.Add(provider);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddEmbeddingGenerator(IEmbeddingGenerator generator)
    {
        _providers.Add(generator);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddMessageGenerator(IMessageGenerator generator)
    {
        _providers.Add(generator);
        return this;
    }

    /// <inheritdoc />
    public IFileServiceBuilder AddFileStorage(IFileStorage storage)
    {
        _storages.Add(storage);
        return Files;
    }

    /// <inheritdoc />
    public IMemoryServiceBuilder AddQueueStorage(IQueueStorage storage)
    {
        _storages.Add(storage);
        return Memory;
    }

    /// <inheritdoc />
    public IMemoryServiceBuilder AddVectorStorage(IVectorStorage storage)
    {
        _storages.Add(storage);
        return Memory;
    }

    /// <inheritdoc />
    public IHiveService Build()
    {
        var provider = Services.BuildServiceProvider();
        return new HiveService(provider);
    }
}
