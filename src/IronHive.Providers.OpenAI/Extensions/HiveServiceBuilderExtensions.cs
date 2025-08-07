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
        builder.AddMessageGenerator(new OpenAIMessageGenerator(config)
        {
            ProviderName = name
        });
        builder.AddEmbeddingGenerator(new OpenAIEmbeddingGenerator(config)
        { 
            ProviderName = name 
        });
        return builder;
    }
}
