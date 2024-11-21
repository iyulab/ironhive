using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Extensions;
using Raggle.Driver.Ollama.Configurations;

namespace Raggle.Driver.Ollama;

public static partial class ServiceCollectionExtension
{
    public static IServiceCollection AddOllamaServices(
        this IServiceCollection services,
        object key,
        OllamaConfig config)
    {
        return services.AddChatCompletionService(key, new OllamaChatCompletionService(config))
                       .AddEmbeddingService(key, new OllamaEmbeddingService(config));
    }
}
