using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Extensions;
using Raggle.Driver.OpenAI.Configurations;

namespace Raggle.Driver.OpenAI;

public static partial class ServiceCollectionExtension
{
    public static IServiceCollection AddOpenAIServices(
        this IServiceCollection services,
        object key,
        OpenAIConfig config)
    {
        return services.AddChatCompletionService(key, new OpenAIChatCompletionService(config))
                       .AddEmbeddingService(key, new OpenAIEmbeddingService(config));
    }
}
