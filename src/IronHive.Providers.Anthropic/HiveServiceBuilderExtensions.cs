using IronHive.Abstractions;

namespace IronHive.Providers.Anthropic;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Adds the Anthropic providers to the Hive service builder.
    /// </summary>
    public static IHiveServiceBuilder AddAnthropicProviders(
        this IHiveServiceBuilder builder,
        string name, 
        AnthropicConfig config)
    {
        builder.AddModelCatalogProvider(new AnthropicModelCatalogProvider(config)
        { 
            ProviderName = name
        });
        builder.AddMessageGenerationProvider(new AnthropicMessageGenerationProvider(config)
        {
            ProviderName = name
        });
        return builder;
    }
}
