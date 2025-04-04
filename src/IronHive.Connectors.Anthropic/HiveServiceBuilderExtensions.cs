using IronHive.Abstractions;

namespace IronHive.Connectors.Anthropic;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Adds the Anthropic connectors to the Hive service builder.
    /// </summary>
    public static IHiveServiceBuilder AddAnthropicConnectors(
        this IHiveServiceBuilder builder,
        string serviceKey, 
        AnthropicConfig config)
    {
        builder.AddChatCompletionConnector(serviceKey, new AnthropicChatCompletionConnector(config));
        return builder;
    }
}
