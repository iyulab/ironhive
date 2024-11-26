using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Extensions;
using Raggle.Driver.Anthropic.Configurations;

namespace Raggle.Driver.Anthropic;

public static partial class IServiceCollectionExtensions
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
