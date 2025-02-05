using Raggle.Driver.Anthropic;
using Raggle.Driver.Anthropic.Configurations;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAnthropicServices(
        this IServiceCollection services,
        string serviceKey,
        AnthropicConfig config)
    {
        var chatCompletionService = new AnthropicChatCompletionService(config);
        return services.AddChatCompletionService(serviceKey, chatCompletionService);
    }
}
