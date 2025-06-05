using IronHive.Providers.Ollama;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Adds Ollama providers to the Hive service builder.
    /// </summary>
    public static IHiveServiceBuilder AddOllamaProviders(
        this IHiveServiceBuilder builder,
        string name,
        OllamaConfig config)
    {
        builder.AddModelCatalogProvider(new OllamaModelCatalogProvider(config)
        {
            ProviderName = name
        });
        builder.AddMessageGenerationProvider(new OllamaMessageGenerationProvider(config)
        {
            ProviderName = name
        });
        builder.AddEmbeddingProvider(new OllamaEmbeddingProvider(config)
        {
            ProviderName = name
        });
        return builder;
    }
}
