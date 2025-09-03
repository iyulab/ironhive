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
using IronHive.Abstractions.Collections;
using IronHive.Core.Collections;

namespace IronHive.Core;

/// <inheritdoc />
public class HiveServiceBuilder : IHiveServiceBuilder
{
    private readonly IProviderCollection _providers;
    private readonly IStorageCollection _storages;
    private readonly IToolCollection _tools;

    public HiveServiceBuilder(IServiceCollection? services = null)
    {
        Services = services ?? new ServiceCollection();

        // 등록된 Provider 및 Storage 관리
        _providers = new ProviderCollection();
        _storages = new StorageCollection();
        _tools = new ToolCollection();
        Services.AddSingleton(_providers);
        Services.AddSingleton(_storages);
        Services.AddSingleton(_tools);

        // AI 서비스
        Services.TryAddSingleton<IModelCatalogService>();
        Services.TryAddSingleton<IEmbeddingService>();
        Services.TryAddSingleton<IMessageService>();

        // 서비스 빌더
        Agent = new AgentServiceBuilder(Services);
        File = new FileServiceBuilder(Services);
        Memory = new MemoryServiceBuilder(Services);
    }

    /// <inheritdoc />
    public IServiceCollection Services { get; }

    /// <inheritdoc />
    public IAgentServiceBuilder Agent { get; }

    /// <inheritdoc />
    public IFileServiceBuilder File { get; }

    /// <inheritdoc />
    public IMemoryServiceBuilder Memory { get; }

    /// <inheritdoc />
    public IHiveServiceBuilder AddModelCatalog(string providerName, IModelCatalog provider)
    {
        _providers.TryAdd<IModelCatalog, IModelCatalog>(providerName, provider);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddEmbeddingGenerator(string providerName, IEmbeddingGenerator generator)
    {
        _providers.TryAdd<IEmbeddingGenerator, IEmbeddingGenerator>(providerName, generator);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddMessageGenerator(string providerName, IMessageGenerator generator)
    {
        _providers.TryAdd<IMessageGenerator, IMessageGenerator>(providerName, generator);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddFileStorage(string storageName, IFileStorage storage)
    {
        _storages.TryAdd<IFileStorage, IFileStorage>(storageName, storage);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddQueueStorage(string storageName, IQueueStorage storage)
    {
        _storages.TryAdd<IQueueStorage, IQueueStorage>(storageName, storage);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddVectorStorage(string storageName, IVectorStorage storage)
    {
        _storages.TryAdd<IVectorStorage, IVectorStorage>(storageName, storage);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddTool(ITool tool)
    {
        _tools.Add(tool);
        return this;
    }

    /// <inheritdoc />
    public IHiveService Build()
    {
        var provider = Services.BuildServiceProvider();
        return new HiveService(provider);
    }
}
