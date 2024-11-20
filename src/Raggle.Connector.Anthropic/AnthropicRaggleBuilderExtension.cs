using Raggle.Abstractions;
using Raggle.Connector.Anthropic.Configurations;

namespace Raggle.Connector.Anthropic;

public static class AnthropicRaggleBuilderExtension
{
    public static void AddAnthropicServices(
        this IRaggleBuilder builder, 
        object key,
        AnthropicConfig config)
    {
        builder.AddChatCompletionService(key, new AnthropicChatCompletionService(config));
    }
}
