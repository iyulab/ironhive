using Microsoft.Extensions.DependencyInjection;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Tools;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IronHive.Abstractions;

namespace IronHive.Core;

public abstract class HiveBuilderBase<TBuilder> where TBuilder : HiveBuilderBase<TBuilder>
{
    protected readonly IHiveServiceStore _store;
    protected readonly IServiceCollection _services;
    
    protected HiveBuilderBase(IServiceCollection services)
    {
        _services = services;
        _store = new HiveServiceStore();
        _services.AddSingleton(_store);
    }

    public TBuilder AddConnector(string key, IChatCompletionConnector connector)
    {
        _store.AddService(key, connector);
        return (TBuilder)this;
    }

    public TBuilder AddConnector(string key, IEmbeddingConnector connector)
    {
        _store.AddService(key, connector);
        return (TBuilder)this;
    }

    public TBuilder AddTool<TService>(
        string key,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Func<IServiceProvider, object?, TService>? implementationFactory = null)
        where TService : class, IToolHandler
    {
        AddKeyedService<IToolHandler, TService>(key, lifetime, implementationFactory);
        return (TBuilder)this;
    }

    public TBuilder WithStorage(IVectorStorage storage)
    {
        _services.RemoveAll<IVectorStorage>();
        _services.AddSingleton(storage);
        return (TBuilder)this;
    }

    public TBuilder WithStorage(IQueueStorage storage)
    {
        _services.RemoveAll<IQueueStorage>();
        _services.AddSingleton(storage);
        return (TBuilder)this;
    }

    public TBuilder AddPipeline<TService>(
        string key,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Func<IServiceProvider, object?, TService>? implementationFactory = null)
        where TService : class, IPipelineHandler
    {
        AddKeyedService<IPipelineHandler, TService>(key, lifetime, implementationFactory);
        return (TBuilder)this;
    }

    // Keyed Service 등록 로직 처리
    protected void AddKeyedService<TService, TImplementation>(
        string key,
        ServiceLifetime lifetime,
        Func<IServiceProvider, object?, TImplementation>? implementationFactory = null)
        where TService : class
        where TImplementation : class, TService
    {
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                if (implementationFactory != null)
                    _services.AddKeyedSingleton<TService, TImplementation>(key, implementationFactory);
                else
                    _services.AddKeyedSingleton<TService, TImplementation>(key);
                break;
            case ServiceLifetime.Scoped:
                if (implementationFactory != null)
                    _services.AddKeyedScoped<TService, TImplementation>(key, implementationFactory);
                else
                    _services.AddKeyedScoped<TService, TImplementation>(key);
                break;
            case ServiceLifetime.Transient:
                if (implementationFactory != null)
                    _services.AddKeyedTransient<TService, TImplementation>(key, implementationFactory);
                else
                    _services.AddKeyedTransient<TService, TImplementation>(key);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }
    }
}
