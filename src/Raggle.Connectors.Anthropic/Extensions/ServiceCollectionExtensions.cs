using Raggle.Connectors.Anthropic;
using Raggle.Connectors.Anthropic.Configurations;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAnthropicServices(
        this IServiceCollection services,
        string serviceKey,
        AnthropicConfig config)
    {
        // Anthropic chat completion
        services.AddChatCompletionConnector(
            serviceKey, 
            new AnthropicChatCompletionAdapter(config));

        return services;
    }
}
