using IronHive.Providers.Anthropic;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Adds the Anthropic providers to the Hive service builder.
    /// </summary>
    public static IHiveServiceBuilder AddAnthropicProviders(
        this IHiveServiceBuilder builder,
        string providerName, 
        AnthropicConfig config)
    {
        builder.AddModelCatalog(providerName, new AnthropicModelCatalog(config));
        builder.AddMessageGenerator(providerName, new AnthropicMessageGenerator(config));
        return builder;
    }
}
