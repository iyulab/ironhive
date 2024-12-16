using Raggle.Abstractions.AI;
using Raggle.Abstractions.Assistant;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Tools;

namespace Raggle.Abstractions;

public interface IRaggle
{
    IServiceProvider Services { get; }

    IRaggleMemory Memory { get; }

    IDocumentStorage GetDocumentStorage();

    IVectorStorage GetVectorStorage();

    IChatCompletionService GetChatCompletionService(string serviceKey);

    IEmbeddingService GetEmbeddingService(string serviceKey);

    IRaggleAssistant CreateAssistant(
        AssistantOptions options,
        string? id = null,
        string? name = null,
        string? description = null,
        string? instruction = null,
        FunctionToolCollection? tools = null);
}
