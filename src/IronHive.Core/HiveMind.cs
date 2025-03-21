using Microsoft.Extensions.DependencyInjection;
using IronHive.Abstractions;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.ChatCompletion.Messages;
using IronHive.Abstractions.Embedding;

namespace IronHive.Core;

public class HiveMind : IHiveMind
{
    public IServiceProvider Services { get; }

    public IChatCompletionService ChatCompletion { get; }

    public IEmbeddingService Embedding { get; }

    public HiveMind(IServiceProvider services)
    {
        Services = services;
        ChatCompletion = services.GetRequiredService<IChatCompletionService>();
        Embedding = services.GetRequiredService<IEmbeddingService>();
    }

    public IHiveSession CreateSession()
    {
        return new HiveSession();
    }

    public HiveAgent CreateAgent()
    {
        throw new NotImplementedException();
    }
}
