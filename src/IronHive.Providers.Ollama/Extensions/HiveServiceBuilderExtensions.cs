using IronHive.Providers.Ollama;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Adds Ollama providers to the Hive service builder.
    /// </summary>
    public static IHiveServiceBuilder AddOllamaProviders(
        this IHiveServiceBuilder builder,
        string providerName,
        OllamaConfig config)
    {
        builder.AddModelCatalog(providerName, new OllamaModelCatalog(config));
        builder.AddMessageGenerator(providerName, new OllamaMessageGenerator(config));
        builder.AddEmbeddingGenerator(providerName, new OllamaEmbeddingGenerator(config));
        return builder;
    }
}
