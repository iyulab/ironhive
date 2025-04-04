using IronHive.Abstractions;

namespace IronHive.Connectors.Ollama;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Adds Ollama connectors to the Hive service builder.
    /// </summary>
    public static IHiveServiceBuilder AddOllamaConnectors(
        this IHiveServiceBuilder builder,
        string serviceKey,
        OllamaConfig config)
    {
        builder.AddChatCompletionConnector(serviceKey, new OllamaChatCompletionConnector(config));
        builder.AddEmbeddingConnector(serviceKey, new OllamaEmbeddingConnector(config));
        return builder;
    }
}
