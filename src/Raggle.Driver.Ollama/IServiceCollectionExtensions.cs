using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Extensions;
using Raggle.Driver.Ollama.Configurations;

namespace Raggle.Driver.Ollama;

public static partial class IServiceCollectionExtensions
{
    public static IServiceCollection AddOllamaServices(
        this IServiceCollection services,
        string serviceKey,
        OllamaConfig config)
    {
        var chatCompletionService = new OllamaChatCompletionService(config);
        var embeddingService = new OllamaEmbeddingService(config);

        return services.AddChatCompletionService(serviceKey, chatCompletionService)
                       .AddEmbeddingService(serviceKey, embeddingService);
    }
}
