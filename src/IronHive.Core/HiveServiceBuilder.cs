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
using IronHive.Storages.Local;
using IronHive.Core.Files.Decoders;
using IronHive.Core.Memory.Handlers;
using IronHive.Storages.LiteDB;

namespace IronHive.Core;

public class HiveServiceBuilder : IHiveServiceBuilder
{
    private readonly HiveServiceStore _store;
    private readonly IServiceCollection _services;

    public HiveServiceBuilder(IServiceCollection? services = null)
    {
        _services = services ?? new ServiceCollection();
        _store = new HiveServiceStore();
    }

    /// <inheritdoc />
    public IHiveMind BuildHiveMind()
    {
        AddDefaultServices();
        var provider = _services.BuildServiceProvider();
        return new HiveMind(provider);
    }

    // 기본 서비스 테스트 중...
    public IHiveServiceBuilder AddDefaultServices()
    {
        // 채팅 기본 서비스 등록
        _services.TryAddSingleton<IHiveServiceStore>(_store);
        _services.TryAddSingleton<IHiveMind, HiveMind>();
        _services.TryAddSingleton<IChatCompletionService, ChatCompletionService>();
        _services.TryAddSingleton<IEmbeddingService, EmbeddingService>();
        _services.TryAddSingleton<IToolHandlerManager, ToolHandlerManager>();

        // RAG 기본 서비스 등록
        _services.TryAddSingleton<IHiveMemory, HiveMemory>();
        _services.TryAddSingleton<IFileStorageFactory, FileStorageFactory>();
        _services.TryAddSingleton<IFileManager, FileManager>();
        _services.TryAddSingleton<IPipelineOrchestrator, PipelineOrchestrator>();

        // 테스트용
        WithQueueStorage(new LocalQueueStorage());
        WithPipelineStorage(new LocalPipelineStorage());
        WithVectorStorage(new LiteDBVectorStorage(new LiteDBConfig
        {
            DatabasePath = "c:\\temp\\vector.db"
        }));

        AddFileDecoder(new TextDecoder());
        AddFileDecoder(new WordDecoder());
        AddFileDecoder(new PDFDecoder());
        AddFileDecoder(new PPTDecoder());
        AddFileDecoder(new ImageDecoder());

        AddPipelineHandler<DecodeHandler>("decode");
        AddPipelineHandler<ChunkHandler>("chunk");
        AddPipelineHandler<QnAGenHandler>("qnagen");
        AddPipelineHandler<EmbedHandler>("embed");

        // Memory Service Validation
        if (!ContainsService<IQueueStorage>())
            throw new InvalidOperationException("Queue storage is not required");
        if (!ContainsService<IPipelineStorage>())
            throw new InvalidOperationException("Pipeline storage is not required");
        if (!ContainsService<IVectorStorage>())
            throw new InvalidOperationException("Vector storage is not required");

        return this;
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
    public IHiveServiceBuilder AddToolHandler<TService>(
        string serviceKey,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Func<IServiceProvider, object?, TService>? implementationFactory = null)
        where TService : class, IToolHandler
    {
        AddKeyedService<IToolHandler, TService>(serviceKey, lifetime, implementationFactory);
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
    public IHiveServiceBuilder AddPipelineHandler<TService>(
        string serviceKey,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Func<IServiceProvider, object?, TService>? implementationFactory = null)
        where TService : class, IPipelineHandler
    {
        AddKeyedService<IPipelineHandler, TService>(serviceKey, lifetime, implementationFactory);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddFileDecoder(IFileDecoder decoder)
    {
        _services.AddSingleton(decoder);
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
    private bool ContainsService<TService>()
    {
        return _services.Any(x => x.ServiceType == typeof(TService));
    }
}
