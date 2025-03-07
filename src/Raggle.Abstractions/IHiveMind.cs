using Raggle.Abstractions.ChatCompletion;
using Raggle.Abstractions.Embedding;

namespace Raggle.Abstractions;

public interface IHiveMind
{
    IServiceProvider Services { get; }

    IChatCompletionService ChatCompletion { get; }

    IEmbeddingService Embedding { get; }
}
