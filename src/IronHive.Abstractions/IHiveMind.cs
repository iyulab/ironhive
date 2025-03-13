using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Embedding;

namespace IronHive.Abstractions;

public interface IHiveMind
{
    IServiceProvider Services { get; }

    IChatCompletionService ChatCompletion { get; }

    IEmbeddingService Embedding { get; }
}
