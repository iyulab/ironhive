using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Tools;
using IronHive.Abstractions;
using IronHive.Abstractions.Files;
using IronHive.Core.Services;
using IronHive.Core.Tools;
using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Message;
using IronHive.Abstractions.Storages;
using IronHive.Core.Files;
using IronHive.Core.Memory;

namespace IronHive.Core;

/// <inheritdoc />
public class HiveServiceBuilder : IHiveServiceBuilder
{
    /// <summary>
    /// 임시 서비스 컴포넌트 저장소
    /// </summary>
    private readonly List<object> _components = new();

    public HiveServiceBuilder(IServiceCollection? services = null)
    {
        Services = services ?? new ServiceCollection();
    }

    /// <inheritdoc />
    public IServiceCollection Services { get; }

    /// <inheritdoc />
    public IHiveServiceBuilder AddModelCatalogProvider(IModelCatalogProvider provider)
    {
        _components.Add(provider);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddMessageGenerator(IMessageGenerator generator)
    {
        _components.Add(generator);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddEmbeddingGenerator(IEmbeddingGenerator generator)
    {
        _components.Add(generator);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddToolPlugin(IToolPlugin plugin)
    {
        _components.Add(plugin);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddFileStorage(IFileStorage storage)
    {
        _components.Add(storage);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddFileDecoder(IFileDecoder decoder)
    {
        Services.AddSingleton(decoder);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder WithVectorStorage(IVectorStorage storage)
    {
        Services.RemoveAll<IVectorStorage>();
        Services.AddSingleton(storage);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder WithMemoryQueueStorage(IQueueStorage<MemoryPipelineRequest> storage)
    {
        Services.RemoveAll<IQueueStorage<MemoryPipelineRequest>>();
        Services.AddSingleton(storage);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddMemoryPipelineHandler<TImplementation>(
        string serviceKey,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Func<IServiceProvider, object?, TImplementation>? implementationFactory = null)
        where TImplementation : class, IMemoryPipelineHandler
    {
        this.AddKeyedService<IMemoryPipelineHandler, TImplementation>(serviceKey, lifetime, implementationFactory);
        return this;
    }

    /// <inheritdoc />
    public IHiveMind BuildHiveMind()
    {
        // AI 서비스
        Services.TryAddSingleton<IModelCatalogService>(sp =>
        {
            var providers = _components.OfType<IModelCatalogProvider>();
            return new ModelCatalogService(providers);
        });
        Services.TryAddSingleton<IMessageGenerationService>(sp =>
        {
            var tools = sp.GetRequiredService<IToolPluginManager>();
            var generators = _components.OfType<IMessageGenerator>();
            return new MessageGenerationService(tools, generators);
        });
        Services.TryAddSingleton<IEmbeddingGenerationService>(sp =>
        {
            var providers = _components.OfType<IEmbeddingGenerator>();
            return new EmbeddingGenerationService(providers);
        });
        // Tool 서비스
        Services.TryAddSingleton<IToolPluginManager>(sp =>
        {
            var tools = _components.OfType<IToolPlugin>();
            return new ToolPluginManager(tools);
        });
        // File 서비스
        Services.TryAddSingleton<IFileStorageManager>(sp => 
        {
            var storages = _components.OfType<IFileStorage>();
            return new FileStorageManager(storages);
        });
        Services.TryAddSingleton<IFileDecoderManager, FileDecoderManager>();

        // 메모리 서비스
        // !! (TODO) 메모리 서비스는 따로 빌더 패턴으로 구성하는 것이 좋음,
        // 1. SetQueueStorage
        // 2. SetVectorStorage
        // 3. SetEmbedder
        // 4. SetWorkers()  워커설정 객체
        // 5. SetPipeline() 빌더 패턴으로 파이프라인 구성 
        Services.TryAddSingleton<IMemoryService, MemoryService>();
        Services.TryAddSingleton<IMemoryWorkerManager, MemoryWorkerManager>();

        var provider = Services.BuildServiceProvider();
        return new HiveMind(provider);
    }
}
