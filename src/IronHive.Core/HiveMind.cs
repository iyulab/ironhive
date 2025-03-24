using Microsoft.Extensions.DependencyInjection;
using IronHive.Abstractions;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Embedding;

namespace IronHive.Core;

public class HiveMind : IHiveMind
{
    public IChatCompletionService ChatCompletion { get; }

    public IEmbeddingService Embedding { get; }

    public HiveMind(IServiceProvider services)
    {
        ChatCompletion = services.GetRequiredService<IChatCompletionService>();
        Embedding = services.GetRequiredService<IEmbeddingService>();
    }

    public IHiveSession CreateSession()
    {
        throw new NotImplementedException();
    }

    public HiveAgent CreateAgent()
    {
        throw new NotImplementedException();
    }
}
