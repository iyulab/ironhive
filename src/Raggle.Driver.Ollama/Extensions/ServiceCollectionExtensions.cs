using Raggle.Driver.Ollama;
using Raggle.Driver.Ollama.Configurations;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
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
