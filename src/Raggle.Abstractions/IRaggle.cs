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

    public IRaggleAssistant CreateAssistant(
        string provider,      // required
        string model,         // required
        string? id = null,
        string? name = null,
        string? description = null,
        string? instruction = null,
        int? maxTokens = null,
        float? temperature = null,
        int? topK = null,
        float? topP = null,
        string[]? stopSequences = null,
        FunctionToolCollection? tools = null);
}
