using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Tools;
using IronHive.Abstractions;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Catalog;
using IronHive.Core.Memory;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Queue;
using IronHive.Abstractions.Vector;
using IronHive.Core.Services;
using IronHive.Core.Files;
using IronHive.Core.Tools;
using IronHive.Core.Registries;

namespace IronHive.Core;

/// <inheritdoc />
public class HiveServiceBuilder : IHiveServiceBuilder
{
    private readonly ProviderRegistry _providers = new();
    private readonly StorageRegistry _storages = new();
    private readonly ToolCollection _tools = new();

    public HiveServiceBuilder(IServiceCollection? services = null)
    {
        Services = services ?? new ServiceCollection();

        // 등록된 Provider 및 Storage 관리
        Services.AddSingleton(_providers);
        Services.AddSingleton(_storages);
        Services.AddSingleton(_tools);

        // 기본 서비스 등록
        Services.TryAddSingleton<IFileStorageService, FileStorageService>();
        Services.TryAddSingleton<IModelCatalogService, ModelCatalogService>();
        Services.TryAddSingleton<IMessageService, MessageService>();
        Services.TryAddSingleton<IEmbeddingService, EmbeddingService>();

        // 서비스 빌더
        Memory = new MemoryServiceBuilder(Services);
    }

    /// <inheritdoc />
    public IServiceCollection Services { get; }

    /// <inheritdoc />
    public IMemoryServiceBuilder Memory { get; }

    /// <inheritdoc />
    public IHiveServiceBuilder AddModelCatalog(string providerName, IModelCatalog provider)
    {
        _providers.TryAdd<IModelCatalog>(providerName, provider);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddEmbeddingGenerator(string providerName, IEmbeddingGenerator generator)
    {
        _providers.TryAdd<IEmbeddingGenerator>(providerName, generator);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddMessageGenerator(string providerName, IMessageGenerator generator)
    {
        _providers.TryAdd<IMessageGenerator>(providerName, generator);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddFileStorage(string storageName, IFileStorage storage)
    {
        _storages.TryAdd<IFileStorage>(storageName, storage);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddQueueStorage(string storageName, IQueueStorage storage)
    {
        _storages.TryAdd<IQueueStorage>(storageName, storage);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddVectorStorage(string storageName, IVectorStorage storage)
    {
        _storages.TryAdd<IVectorStorage>(storageName, storage);
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
