using Raggle.Connectors.OpenAI;
using Raggle.Connectors.OpenAI.Configurations;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenAIServices(
        this IServiceCollection services,
        string serviceKey,
        OpenAIConfig config)
    {
        // chat completion
        services.AddChatCompletionConnector(
            serviceKey, 
            new OpenAIChatCompletionAdapter(config));

        // embedding
        services.AddEmbeddingConnector(
            serviceKey, 
            new OpenAIEmbeddingAdapter(config));

        return services;
    }
}
