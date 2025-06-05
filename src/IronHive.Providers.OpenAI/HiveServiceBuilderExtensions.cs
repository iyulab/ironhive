using IronHive.Providers.OpenAI;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Adds OpenAI providers to the Hive service builder.
    /// </summary>
    public static IHiveServiceBuilder AddOpenAIProviders(
        this IHiveServiceBuilder builder, 
        string name,
        OpenAIConfig config)
    {
        builder.AddModelCatalogProvider(new OpenAIModelCatalogProvider(config)
        {
            ProviderName = name
        });
        builder.AddMessageGenerationProvider(new OpenAIMessageGenerationProvider(config)
        {
            ProviderName = name
        });
        builder.AddEmbeddingProvider(new OpenAIEmbeddingProvider(config)
        { 
            ProviderName = name 
        });
        return builder;
    }
}
