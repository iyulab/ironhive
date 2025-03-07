using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions;
using Raggle.Abstractions.ChatCompletion;
using Raggle.Abstractions.ChatCompletion.Messages;
using Raggle.Abstractions.Embedding;

namespace Raggle.Core;

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
}
