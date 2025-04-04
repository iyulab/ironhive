using IronHive.Abstractions;

namespace IronHive.Connectors.OpenAI;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Adds OpenAI connectors to the Hive service builder.
    /// </summary>
    public static IHiveServiceBuilder AddOpenAIConnectors(
        this IHiveServiceBuilder builder,
        string serviceKey, 
        OpenAIConfig config)
    {
        builder.AddChatCompletionConnector(serviceKey, new OpenAIChatCompletionConnector(config));
        builder.AddEmbeddingConnector(serviceKey, new OpenAIEmbeddingConnector(config));
        return builder;
    }
}
