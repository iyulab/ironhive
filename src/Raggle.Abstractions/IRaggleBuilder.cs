using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;

namespace Raggle.Abstractions;

public interface IRaggleBuilder
{
    IRaggleBuilder AddChatCompletionService<T>(object key, T service)
        where T : class, IChatCompletionService;

    IRaggleBuilder AddEmbeddingService<T>(object key, T service)
        where T : class, IEmbeddingService;

    IRaggleBuilder AddDocumentStorage<T>(object key, T service)
        where T : class, IDocumentStorage;

    IRaggleBuilder AddVectorStorage<T>(object key, T service)
        where T : class, IVectorStorage;

    IRaggle Build(RaggleMemoryConfig? config = null);
}
