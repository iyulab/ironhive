using Raggle.Connectors.Ollama;
using Raggle.Connectors.Ollama.Configurations;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOllamaServices(
        this IServiceCollection services,
        string serviceKey,
        OllamaConfig config)
    {
        // chat completion
        services.AddChatCompletionConnector(
            serviceKey, 
            new OllamaChatCompletionAdapter(config));

        // embedding
        services.AddEmbeddingConnector(
            serviceKey, 
            new OllamaEmbeddingAdapter(config));

        return services;
    }
}
