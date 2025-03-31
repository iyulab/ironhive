using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Tools;
using IronHive.Abstractions;
using IronHive.Abstractions.Files;
using IronHive.Core.Files;
using IronHive.Core.Memory;
using IronHive.Core.Services;
using IronHive.Core.Files.Decoders;
using IronHive.Core.Storages;
using IronHive.Storages.LiteDB;
using IronHive.Core.Memory.Handlers;

namespace IronHive.Core;

public class HiveServiceBuilder : IHiveServiceBuilder
{
    private readonly IServiceCollection _services;
    private readonly IHiveServiceStore _store;

    public HiveServiceBuilder(IServiceCollection? services = null, IHiveServiceStore? store = null)
    {
        _services = services ?? new ServiceCollection();
        _store = store ?? new HiveServiceStore();
        _services.AddSingleton<IHiveServiceStore>(_store);
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddChatCompletionConnector(
        string serviceKey,
        IChatCompletionConnector connector)
    {
        _store.AddService(serviceKey, connector);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddEmbeddingConnector(
        string serviceKey,
        IEmbeddingConnector connector)
    {
        _store.AddService(serviceKey, connector);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddToolHandler<TImplementation>(
        string serviceKey,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Func<IServiceProvider, object?, TImplementation>? implementationFactory = null)
        where TImplementation : class, IToolHandler
    {
        AddKeyedService<IToolHandler, TImplementation>(serviceKey, lifetime, implementationFactory);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder WithQueueStorage(IQueueStorage storage)
    {
        _services.RemoveAll<IQueueStorage>();
        _services.AddSingleton(storage);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder WithPipelineStorage(IPipelineStorage storage)
    {
        _services.RemoveAll<IPipelineStorage>();
        _services.AddSingleton(storage);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder WithVectorStorage(IVectorStorage storage)
    {
        _services.RemoveAll<IVectorStorage>();
        _services.AddSingleton(storage);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddPipelineHandler<TImplementation>(
        string serviceKey,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Func<IServiceProvider, object?, TImplementation>? implementationFactory = null)
        where TImplementation : class, IPipelineHandler
    {
        AddKeyedService<IPipelineHandler, TImplementation>(serviceKey, lifetime, implementationFactory);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddFileStorage<TImplementation>(
        string serviceKey, 
        Func<IServiceProvider, object?, TImplementation> storageFactory)
        where TImplementation : class, IFileStorage
    {
        _store.AddFactory<IFileStorage>(serviceKey, storageFactory);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddFileDecoder(IFileDecoder decoder)
    {
        _services.AddSingleton(decoder);
        return this;
    }



    /// <inheritdoc />
    public IHiveMind BuildHiveMind()
    {
        // 채팅 기본 서비스 등록
        _services.TryAddSingleton<IChatCompletionService, ChatCompletionService>();
        _services.TryAddSingleton<IEmbeddingService, EmbeddingService>();
        _services.TryAddSingleton<IToolHandlerManager, ToolHandlerManager>();

        var provider = _services.BuildServiceProvider();
        return new HiveMind(provider);
    }

    /// <inheritdoc />
    public IHiveMemory BuildHiveMemory()
    {
        // Memory Service Validation
        if (!EnsureService<IQueueStorage>())
        {
            WithQueueStorage(new LocalQueueStorage());
        }
        if (!EnsureService<IPipelineStorage>())
        {
            WithPipelineStorage(new LocalPipelineStorage());
        }
        if (!EnsureService<IVectorStorage>())
        {
            WithVectorStorage(new LiteDBVectorStorage(new LiteDBConfig
            {
                DatabasePath = "c:\\temp\\vector.db"
            }));
        }
        if (!EnsureService<IEmbeddingConnector>())
        {
            throw new InvalidOperationException("Embedding connector is required");
        }

        // RAG 기본 서비스 등록
        _services.TryAddSingleton<IFileStorageManager, FileStorageManager>();
        _services.TryAddSingleton<IEmbeddingService, EmbeddingService>();

        // File Decoders
        AddFileDecoder(new TextDecoder());
        AddFileDecoder(new WordDecoder());
        AddFileDecoder(new PDFDecoder());
        AddFileDecoder(new PPTDecoder());
        AddFileDecoder(new ImageDecoder());

        // Pipeline Handlers
        AddPipelineHandler<DecodeHandler>("decode");
        AddPipelineHandler<ChunkHandler>("chunk");
        AddPipelineHandler<QnAGenHandler>("qnagen");
        AddPipelineHandler<EmbedHandler>("embed");

        var provider = _services.BuildServiceProvider();
        return new HiveMemory(
            queue: provider.GetRequiredService<IQueueStorage>(),
            pipeline: provider.GetRequiredService<IPipelineStorage>(),
            vector: provider.GetRequiredService<IVectorStorage>(),
            embedding: provider.GetRequiredService<IEmbeddingService>());
    }

    ///// <inheritdoc />
    //public IHiveMemoryWorker BuildHiveMemoryWorker()
    //{
    //    var provider = _services.BuildServiceProvider();
    //    return new HiveMemoryWorker(provider);
    //}

    // 서비스 주입 테스트(임시 메서드)
    public IHiveServiceBuilder AddDefaultTestServices()
    {
        // 채팅 기본 서비스 등록
        _services.TryAddSingleton<IHiveMind, HiveMind>();
        _services.TryAddSingleton<IChatCompletionService, ChatCompletionService>();
        _services.TryAddSingleton<IEmbeddingService, EmbeddingService>();
        _services.TryAddSingleton<IToolHandlerManager, ToolHandlerManager>();

        // RAG 기본 서비스 등록
        _services.TryAddSingleton<IHiveMemory, HiveMemory>();
        _services.TryAddSingleton<IFileStorageManager, FileStorageManager>();
        _services.TryAddSingleton<IPipelineOrchestrator, PipelineOrchestrator>();

        return this;
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
                _services.AddKeyedSingleton<TService, TImplementation>(serviceKey, implementationFactory);
            else
                _services.AddKeyedSingleton<TService, TImplementation>(serviceKey);
        }
        else if (lifetime == ServiceLifetime.Scoped)
        {
            if (implementationFactory != null)
                _services.AddKeyedScoped<TService, TImplementation>(serviceKey, implementationFactory);
            else
                _services.AddKeyedScoped<TService, TImplementation>(serviceKey);
        }
        else if (lifetime == ServiceLifetime.Transient)
        {
            if (implementationFactory != null)
                _services.AddKeyedTransient<TService, TImplementation>(serviceKey, implementationFactory);
            else
                _services.AddKeyedTransient<TService, TImplementation>(serviceKey);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }
    }

    // 서비스 등록 확인
    private bool EnsureService<TService>()
    {
        var serviceType = typeof(TService);

        if (serviceType == typeof(IChatCompletionConnector))
            return _store.GetServices<IChatCompletionConnector>().Any();
        else if (serviceType == typeof(IEmbeddingConnector))
            return _store.GetServices<IEmbeddingConnector>().Any();
        else if (serviceType == typeof(IFileStorage))
            return _store.GetFactories<IFileStorage>().Any();
        else
            return _services.Any(x => x.ServiceType == typeof(TService));
    }
}
