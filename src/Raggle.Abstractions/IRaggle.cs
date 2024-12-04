using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;

namespace Raggle.Abstractions;

public interface IRaggle
{
    IServiceProvider Services { get; }

    IRaggleMemory Memory { get; }

    T GetKeyedService<T>(string serviceKey) where T : notnull;

    IDocumentStorage GetDocumentStorage();

    IVectorStorage GetVectorStorage();

    IChatCompletionService GetChatCompletionService(string serviceKey);

    IEmbeddingService GetEmbeddingService(string serviceKey);
}
