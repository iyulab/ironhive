using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Tools;
using IronHive.Abstractions;
using IronHive.Abstractions.Files;
using IronHive.Core.Services;
using IronHive.Core.Storages;
using IronHive.Core.Tools;
using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Message;
using IronHive.Abstractions.Storages;

namespace IronHive.Core;

/// <inheritdoc />
public class HiveServiceBuilder : IHiveServiceBuilder
{
    public HiveServiceBuilder(IServiceCollection? services = null)
    {
        Services = services ?? new ServiceCollection();

        // 필수 서비스 없을 경우 등록
        AddRequiredServices();
    }

    public IServiceCollection Services { get; }

    /// <inheritdoc />
    public IHiveServiceBuilder AddModelCatalogProvider(IModelCatalogProvider provider)
    {
        Services.AddSingleton(provider);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddMessageGenerator(IMessageGenerator provider)
    {
        Services.AddSingleton(provider);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddEmbeddingGenerator(IEmbeddingGenerator provider)
    {
        Services.AddSingleton(provider);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddToolPlugin(IToolPlugin plugin)
    {
        Services.AddSingleton(plugin);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddFileStorage(IFileStorage storage)
    {
        Services.AddSingleton(storage);
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
    public IHiveServiceBuilder WithMemoryEmbedder(string provider, string model)
    {
        Services.AddSingleton<IMemoryEmbedder>(sp => new MemoryEmbedder(sp.GetRequiredService<IEmbeddingGenerationService>())
        {
            Model = model,
            Provider = provider
        });
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
    public IHiveServiceBuilder AddMemoryPipelineObserver<TImplementation>(
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Func<IServiceProvider, TImplementation>? implementationFactory = null)
        where TImplementation : class, IMemoryPipelineObserver
    {
        AddService<IMemoryPipelineObserver, TImplementation>(lifetime, implementationFactory);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddMemoryPipelineHandler<TImplementation>(
        string serviceKey,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Func<IServiceProvider, object?, TImplementation>? implementationFactory = null)
        where TImplementation : class, IMemoryPipelineHandler
    {
        AddKeyedService<IMemoryPipelineHandler, TImplementation>(serviceKey, lifetime, implementationFactory);
        return this;
    }

    /// <inheritdoc />
    public IHiveMind BuildHiveMind()
    {
        // 선택 서비스 등록 확인
        if (!ContainsAny<IModelCatalogProvider>())
            Debug.WriteLine("ModelCatalogProvider is not registered. Model will not work.");
        if (!ContainsAny<IMessageGenerator>())
            Debug.WriteLine("MessageGenerationProvider is not registered. Chat Completion will not work.");
        if (!ContainsAny<IEmbeddingGenerator>())
            Debug.WriteLine("EmbeddingConnector is not registered. Embedding will not work.");

        if (!ContainsAny<IFileStorage>())
            Debug.WriteLine("FileStorage is not registered. File Service will not work.");
        if (!ContainsAny<IFileDecoder>())
            Debug.WriteLine("FileDecoder is not registered. File Service will not work.");

        if (!ContainsAny<IMemoryPipelineHandler>())
            Debug.WriteLine("PipelineHandler is not registered. Memory Pipeline will not work.");
        if (!ContainsAny<IMemoryPipelineObserver>())
            Debug.WriteLine("PipelineObserver is not registered. You can't check the pipeline work.");

        var provider = Services.BuildServiceProvider();
        return new HiveMind(provider);
    }

    // 필수 서비스 등록
    private void AddRequiredServices()
    {
        // AI 서비스
        Services.TryAddSingleton<IModelCatalogService, ModelCatalogService>();
        Services.TryAddSingleton<IMessageGenerationService, MessageGenerationService>();
        Services.TryAddSingleton<IEmbeddingGenerationService, EmbeddingGenerationService>();
        Services.TryAddSingleton<IToolPluginManager, ToolPluginManager>();
        // File 서비스
        Services.TryAddSingleton<IFileStorageManager, FileStorageManager>();
        Services.TryAddSingleton<IFileDecoderManager, FileDecoderManager>();

        // 메모리 서비스
        // !! (TODO) 메모리 서비스는 따로 빌더 패턴으로 구성하는 것이 좋음,
        // SetWorker/SetEmbedder/SetQueueStorage/SetVectorStorage 등으로 구성
        Services.TryAddSingleton<IMemoryService, MemoryService>();
        Services.TryAddSingleton<IMemoryPipelineWorker, MemoryPipelineWorker>();
    }

    // Service 등록 로직 처리
    private void AddService<TService, TImplementation>(
        ServiceLifetime? lifetime,
        Func<IServiceProvider, TImplementation>? implementationFactory = null)
        where TService : class
        where TImplementation : class, TService
    {
        if (lifetime == ServiceLifetime.Singleton)
        {
            if (implementationFactory != null)
                Services.AddSingleton<TService, TImplementation>(implementationFactory);
            else
                Services.AddSingleton<TService, TImplementation>();
        }
        else if (lifetime == ServiceLifetime.Scoped)
        {
            if (implementationFactory != null)
                Services.AddScoped<TService, TImplementation>(implementationFactory);
            else
                Services.AddScoped<TService, TImplementation>();
        }
        else if (lifetime == ServiceLifetime.Transient)
        {
            if (implementationFactory != null)
                Services.AddTransient<TService, TImplementation>(implementationFactory);
            else
                Services.AddTransient<TService, TImplementation>();
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }
    }

    // Keyed Service 등록 로직 처리
    private void AddKeyedService<TService, TImplementation>(
        string serviceKey,
        ServiceLifetime? lifetime,
        Func<IServiceProvider, object?, TImplementation>? implementationFactory = null)
        where TService : class
        where TImplementation : class, TService
    {
        if (lifetime == ServiceLifetime.Singleton)
        {
            if (implementationFactory != null)
                Services.AddKeyedSingleton<TService, TImplementation>(serviceKey, implementationFactory);
            else
                Services.AddKeyedSingleton<TService, TImplementation>(serviceKey);
        }
        else if (lifetime == ServiceLifetime.Scoped)
        {
            if (implementationFactory != null)
                Services.AddKeyedScoped<TService, TImplementation>(serviceKey, implementationFactory);
            else
                Services.AddKeyedScoped<TService, TImplementation>(serviceKey);
        }
        else if (lifetime == ServiceLifetime.Transient)
        {
            if (implementationFactory != null)
                Services.AddKeyedTransient<TService, TImplementation>(serviceKey, implementationFactory);
            else
                Services.AddKeyedTransient<TService, TImplementation>(serviceKey);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }
    }

    // 서비스 등록 확인
    private bool ContainsAny<TService>()
    {
        return Services.Any(x => x.ServiceType == typeof(TService));
    }
}
