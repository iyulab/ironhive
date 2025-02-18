using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Experimental;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Tools;
using Raggle.Core.AI;
using Raggle.Core.Memory;

namespace Raggle.Core;

public class Raggle : IRaggle
{
    public IServiceProvider Services { get; }
    public IMemoryService Memory { get; }

    public Raggle(IServiceProvider services)
    {
        Services = services;
        Memory = new MemoryService(services);
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

    public IAgent CreateAssistant(
        string service,
        string model,
        string? id = null, 
        string? name = null,
        string? description = null,
        string? instruction = null,
        ChatCompletionParameters? options = null,
        ToolCollection? tools = null)
    {
        throw new NotImplementedException();
    }
}
