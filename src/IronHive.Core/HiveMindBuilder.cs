using Microsoft.Extensions.DependencyInjection;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions;
using IronHive.Core.Services;
using IronHive.Core.Memory;
using IronHive.Abstractions.Tools;
using IronHive.Abstractions.Files;
using IronHive.Core.Files;
using IronHive.Core.Tools;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IronHive.Core;

public class HiveMindBuilder : IHiveMindBuilder
{
    private readonly HiveServiceStore _store = new();
    private readonly ServiceCollection _services = new();

    public IHiveMindBuilder AddChatCompletionConnector(
        string serviceKey,
        IChatCompletionConnector connector)
    {
        _store.AddService(serviceKey, connector);
        return this;
    }

    public IHiveMindBuilder AddEmbeddingConnector(
        string serviceKey,
        IEmbeddingConnector connector)
    {
        _store.AddService(serviceKey, connector);
        return this;
    }

    public IHiveMindBuilder AddToolHandler<TService>(
        string serviceKey,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Func<IServiceProvider, object?, TService>? implementationFactory = null)
        where TService : class, IToolHandler
    {
        AddKeyedService<IToolHandler, TService>(serviceKey, lifetime, implementationFactory);
        return this;
    }

    public IHiveMindBuilder WithVectorStorage(IVectorStorage storage)
    {
        _services.RemoveAll<IVectorStorage>();
        _services.AddSingleton(storage);
        return this;
    }

    public IHiveMindBuilder WithQueueStorage(IQueueStorage storage)
    {
        _services.RemoveAll<IQueueStorage>();
        _services.AddSingleton(storage);
        return this;
    }

    public IHiveMindBuilder AddPipelineHandler<TService>(
        string serviceKey,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Func<IServiceProvider, object?, TService>? implementationFactory = null)
        where TService : class, IPipelineHandler
    {
        AddKeyedService<IPipelineHandler, TService>(serviceKey, lifetime, implementationFactory);
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

    public IHiveMind Build()
    {
        // 코어 서비스 등록
        _services.AddSingleton<IHiveServiceStore>(_store);
        _services.AddSingleton<IChatCompletionService, ChatCompletionService>();
        _services.AddSingleton<IEmbeddingService, EmbeddingService>();
        _services.AddSingleton<IToolManager, ToolManager>();

        _services.AddSingleton<IMemoryService, MemoryService>();
        //_services.AddSingleton<IPipelineOrchestrator, PipelineOrchestrator>();
        _services.AddSingleton<IFileStorageFactory, FileStorageFactory>();
        //_services.AddSingleton<IFileManager, FileManager>();

        var provider = _services.BuildServiceProvider();
        return new HiveMind(provider);
    }
}