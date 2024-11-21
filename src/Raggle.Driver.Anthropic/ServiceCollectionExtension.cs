using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Extensions;
using Raggle.Driver.Anthropic.Configurations;

namespace Raggle.Driver.Anthropic;

public static partial class ServiceCollectionExtension
{
    public static IServiceCollection AddAnthropicServices(
        this IServiceCollection services, 
        object key,
        AnthropicConfig config)
    {
        return services.AddChatCompletionService(key, new AnthropicChatCompletionService(config));
    }
}
