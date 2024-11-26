using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Extensions;
using Raggle.Driver.OpenAI.Configurations;

namespace Raggle.Driver.OpenAI;

public static partial class IServiceCollectionExtensions
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
