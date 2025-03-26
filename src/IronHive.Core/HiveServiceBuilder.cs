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
using IronHive.Core.Tools;
using IronHive.Core;

namespace Microsoft.Extensions.DependencyInjection;

public class HiveServiceBuilder : IHiveServiceBuilder
{
    private readonly HiveServiceStore _store;
    private readonly IServiceCollection _services;

    public HiveServiceBuilder(IServiceCollection services)
    {
        _services = services;
        _store = new HiveServiceStore();
        _services.AddSingleton<IHiveServiceStore>(_store);
    }

    public IHiveServiceBuilder AddChatCompletionConnector(
        string serviceKey,
        IChatCompletionConnector connector)
    {
        _store.AddService(serviceKey, connector);
        return this;
    }

    public IHiveServiceBuilder AddEmbeddingConnector(
        string serviceKey,
        IEmbeddingConnector connector)
    {
        _store.AddService(serviceKey, connector);
        return this;
    }

    public IHiveServiceBuilder AddToolHandler<TService>(
        string serviceKey,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Func<IServiceProvider, object?, TService>? implementationFactory = null)
        where TService : class, IToolHandler
    {
        AddKeyedService<IToolHandler, TService>(serviceKey, lifetime, implementationFactory);
        return this;
    }

    public IHiveServiceBuilder WithVectorStorage(IVectorStorage storage)
    {
        _services.RemoveAll<IVectorStorage>();
        _services.AddSingleton(storage);
        return this;
    }

    public IHiveServiceBuilder WithQueueStorage(IQueueStorage storage)
    {
        _services.RemoveAll<IQueueStorage>();
        _services.AddSingleton(storage);
        return this;
    }

    public IHiveServiceBuilder AddPipelineHandler<TService>(
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
}

public static class ServiceCollectionExtensions
{
    public static IHiveServiceBuilder AddHiveServiceCore(this IServiceCollection services)
    {
        // 코어 서비스 등록
        services.AddSingleton<IHiveMind, HiveMind>();
        services.AddSingleton<IChatCompletionService, ChatCompletionService>();
        services.AddSingleton<IEmbeddingService, EmbeddingService>();
        services.AddSingleton<IToolManager, ToolManager>();

        services.AddSingleton<IMemoryService, MemoryService>();
        //services.AddSingleton<IPipelineOrchestrator, PipelineOrchestrator>();
        services.AddSingleton<IFileStorageFactory, FileStorageFactory>();
        //services.AddSingleton<IFileManager, FileManager>();

        return new HiveServiceBuilder(services);
    }
}
