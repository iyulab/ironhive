using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Assistant;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Tools;
using Raggle.Core.Assistant;
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

    public IRaggleAssistant CreateAssistant(
        string service,
        string model,
        string? id = null, 
        string? name = null,
        string? description = null,
        string? instruction = null,
        ExecuteOptions? options = null,
        FunctionToolCollection? tools = null)
    {
        return new BasicRaggleAssistant(Services)
        {
            Service = service,
            Model = model,
            Id = id,
            Name = name,
            Description = description,
            Instruction = instruction,
            Options = options,
            Tools = tools
        };
    }
}
