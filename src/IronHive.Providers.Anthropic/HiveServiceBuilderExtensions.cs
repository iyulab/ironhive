using IronHive.Providers.Anthropic;

namespace IronHive.Abstractions;

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
        builder.AddMessageGenerator(new AnthropicMessageGenerator(config)
        {
            ProviderName = name
        });
        return builder;
    }
}
