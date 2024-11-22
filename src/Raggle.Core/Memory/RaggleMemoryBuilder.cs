using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Extensions;
using Raggle.Abstractions.Memory;

namespace Raggle.Core.Memory;

public class RaggleMemoryBuilder : IRaggleMemoryBuilder
{
    private object? _documentStorageKey;
    private object? _vectorStorageKey;

    public IServiceCollection Services { get; } = new ServiceCollection();

    public RaggleMemoryBuilder(IServiceCollection? service = null)
    {
        if (service != null)
        {
            Services.CopyRaggleServicesFrom(service);
        }
    }

    public IRaggleMemoryBuilder UseDocumentStorage(object key)
    {
        _documentStorageKey = key;
        return this;
    }

    public IRaggleMemoryBuilder UseVectorStorage(object key)
    {
        _vectorStorageKey = key;
        return this;
    }

    public IRaggleMemoryBuilder AddDocumentDecoder<T>()
        where T : class, IDocumentDecoder
    {
        Services.AddSingleton<IDocumentDecoder, T>();
        return this;
    }

    public IRaggleMemoryBuilder AddPipelineHandler<T>(object key)
        where T : class, IPipelineHandler
    {
        Services.AddKeyedSingleton<IPipelineHandler, T>(key);
        return this;
    }

    public IRaggleMemory Build()
    {
        var provider = Services.BuildServiceProvider();
        return new RaggleMemory(provider);
    }
}
