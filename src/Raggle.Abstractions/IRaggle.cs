using Raggle.Abstractions.AI;
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

    IAssistant CreateAssistant(
        string service,
        string model,
        string? id = null,
        string? name = null,
        string? description = null,
        string? instruction = null,
        ChatCompletionParameters? options = null,
        ToolCollection? tools = null);
}
