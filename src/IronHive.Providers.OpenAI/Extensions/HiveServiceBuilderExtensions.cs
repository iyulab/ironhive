using IronHive.Providers.OpenAI;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Adds OpenAI providers to the Hive service builder.
    /// </summary>
    public static IHiveServiceBuilder AddOpenAIProviders(
        this IHiveServiceBuilder builder, 
        string providerName,
        OpenAIConfig config)
    {
        builder.AddModelCatalog(providerName, new OpenAIModelCatalog(config));
        builder.AddMessageGenerator(providerName, new OpenAIMessageGenerator(config));
        builder.AddEmbeddingGenerator(providerName, new OpenAIEmbeddingGenerator(config));
        return builder;
    }
}
