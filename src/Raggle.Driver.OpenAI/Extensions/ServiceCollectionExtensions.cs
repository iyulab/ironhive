using Raggle.Driver.OpenAI;
using Raggle.Driver.OpenAI.Configurations;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenAIServices(
        this IServiceCollection services,
        string serviceKey,
        OpenAIConfig config)
    {
        var chatCompletionService = new OpenAIChatCompletionService(config);
        var embeddingService = new OpenAIEmbeddingService(config);

        return services.AddChatCompletionService(serviceKey, chatCompletionService)
                       .AddEmbeddingService(serviceKey, embeddingService);
    }
}
