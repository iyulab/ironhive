using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Tools;
using IronHive.Abstractions;
using IronHive.Abstractions.Files;
using IronHive.Core.Services;
using IronHive.Core.Storages;
using System.Diagnostics;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using System.Net.Sockets;
using System.Threading.Tasks;
using ModelContextProtocol.Protocol.Transport;

namespace IronHive.Core;

/// <inheritdoc />
public class HiveServiceBuilder : IHiveServiceBuilder
{
    private readonly IServiceCollection _services;
    private readonly IHiveServiceStore _store;

    public HiveServiceBuilder(IServiceCollection? services = null, IHiveServiceStore? store = null)
    {
        _services = services ?? new ServiceCollection();
        _store = store ?? new HiveServiceStore();

        // 필수 서비스 없을 경우 등록
        AddRequiredServices();
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
    public IHiveServiceBuilder WithVectorStorage(IVectorStorage storage)
    {
        _services.RemoveAll<IVectorStorage>();
        _services.AddSingleton(storage);
        return this;
    }

    /// <inheritdoc />
    public IHiveServiceBuilder AddPipelineEventHandler(IPipelineEventHandler handler)
    {
        _services.AddSingleton(handler);
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
        Func<IServiceProvider, object?, TImplementation>? implementationFactory = null)
        where TImplementation : class, IFileStorage
    {
        implementationFactory ??= (sp, _) => ActivatorUtilities.CreateInstance<TImplementation>(sp);
        _store.AddFactory<IFileStorage>(serviceKey, implementationFactory);
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
        // 선택 서비스 등록 확인
        if (!ContainsAny<IChatCompletionConnector>())
            Debug.WriteLine("ChatCompletionConnector is not registered. Chat Completion will not work.");
        if (!ContainsAny<IEmbeddingConnector>())
            Debug.WriteLine("EmbeddingConnector is not registered. Embedding will not work.");
        if (!ContainsAny<IToolHandler>())
            Debug.WriteLine("ToolHandler is not registered. Chat Completion Tool Service will not work.");

        if (!ContainsAny<IFileStorage>())
            Debug.WriteLine("FileStorage is not registered. File Service will not work.");
        if (!ContainsAny<IFileDecoder>())
            Debug.WriteLine("FileDecoder is not registered. File Service will not work.");

        if (!ContainsAny<IPipelineHandler>())
            Debug.WriteLine("PipelineHandler is not registered. Memory Pipeline will not work.");
        if (!ContainsAny<IPipelineEventHandler>())
            Debug.WriteLine("PipelineEventHandler is not registered. You can't check the pipeline work.");

        var provider = _services.BuildServiceProvider();
        return new HiveMind(provider);
    }

    // 기본 필수 서비스 등록
    private void AddRequiredServices()
    {
        // Connectors, FileStorage 인스턴스 컨테이너
        _services.AddSingleton<IHiveServiceStore>(_store);
        // AI 서비스
        _services.TryAddSingleton<IChatCompletionService, ChatCompletionService>();
        _services.TryAddSingleton<IEmbeddingService, EmbeddingService>();
        _services.TryAddSingleton<IToolHandlerManager, ToolHandlerManager>();
        // File 서비스
        _services.TryAddSingleton<IFileStorageManager, FileStorageManager>();
        _services.TryAddSingleton<IFileDecoderResolver, FileDecoderResolver>();
        // 메모리 서비스
        _services.TryAddSingleton<IQueueStorage, LocalQueueStorage>();
        _services.TryAddSingleton<IVectorStorage, LocalVectorStorage>();
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
    private bool ContainsAny<TService>()
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
