using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;
using Raggle.Core.Memory;

namespace Raggle.Core;

public class Raggle : IRaggle
{
    public IServiceProvider Services { get; }
    public IRaggleMemory Memory { get; }

    public Raggle(IServiceProvider services)
    {
        Services = services;
        Memory = new RaggleMemory(services);
    }

    public T GetKeyedService<T>(string serviceKey) where T : notnull
        => Services.GetRequiredKeyedService<T>(serviceKey);

    public IDocumentStorage GetDocumentStorage()
        => Services.GetRequiredService<IDocumentStorage>();

    public IVectorStorage GetVectorStorage()
        => Services.GetRequiredService<IVectorStorage>();

    public IChatCompletionService GetChatCompletionService(string serviceKey)
        => Services.GetRequiredKeyedService<IChatCompletionService>(serviceKey);

    public IEmbeddingService GetEmbeddingService(string serviceKey)
        => Services.GetRequiredKeyedService<IEmbeddingService>(serviceKey);
}
