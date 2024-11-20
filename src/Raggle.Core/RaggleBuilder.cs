using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;
using Raggle.Core.Memory;

namespace Raggle.Core;

public class RaggleBuilder : IRaggleBuilder
{
    private readonly IServiceCollection _innerServices = new ServiceCollection();
    private readonly IServiceCollection? _externalServices;

    public RaggleBuilder(IServiceCollection? services = null)
    {
        _externalServices = services;
    }

    public IRaggleBuilder AddChatCompletionService<T>(object key, T service)
        where T : class, IChatCompletionService
    {
        _innerServices.AddKeyedSingleton<IChatCompletionService>(key, service);
        _externalServices?.AddKeyedSingleton<IChatCompletionService>(key, service);
        return this;
    }

    public IRaggleBuilder AddEmbeddingService<T>(object key, T service)
        where T : class, IEmbeddingService
    {
        _innerServices.AddKeyedSingleton<IEmbeddingService>(key, service);
        _externalServices?.AddKeyedSingleton<IEmbeddingService>(key, service);
        return this;
    }

    public IRaggleBuilder AddDocumentStorage<T>(object key, T service)
        where T : class, IDocumentStorage
    {
        _innerServices.AddKeyedSingleton<IDocumentStorage>(key, service);
        _externalServices?.AddKeyedSingleton<IDocumentStorage>(key, service);
        return this;
    }

    public IRaggleBuilder AddVectorStorage<T>(object key, T service)
        where T : class, IVectorStorage
    {
        _innerServices.AddKeyedSingleton<IVectorStorage>(key, service);
        _externalServices?.AddKeyedSingleton<IVectorStorage>(key, service);
        return this;
    }

    public IRaggle Build(RaggleMemoryConfig? config = null)
    {
        var provider = _innerServices.BuildServiceProvider();
        if (config == null)
        {
            return new Raggle()
            {
                Services = provider
            };
        }
        else
        {
            return new Raggle()
            {
                Services = provider,
                Memory = new RaggleMemory(provider, config)
            };
        }
    }
}
