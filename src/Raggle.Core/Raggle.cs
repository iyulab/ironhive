using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Tools;

namespace Raggle.Core;

public class Raggle : IRaggle
{
    public required IServiceProvider Services { get; init; }
    public IRaggleMemory? Memory { get; init; }
    public ToolKitCollection ToolKits { get; init; } = new();

    public IChatCompletionService GetChatCompletionService(object key)
        => Services.GetRequiredKeyedService<IChatCompletionService>(key);

    public IEnumerable<IChatCompletionService> GetChatCompletionServices()
        => Services.GetKeyedServices<IChatCompletionService>(KeyedService.AnyKey);

    public IEmbeddingService GetEmbeddingService(object key)
        => Services.GetRequiredKeyedService<IEmbeddingService>(key);

    public IEnumerable<IEmbeddingService> GetEmbeddingServices()
        => Services.GetKeyedServices<IEmbeddingService>(KeyedService.AnyKey);


}
