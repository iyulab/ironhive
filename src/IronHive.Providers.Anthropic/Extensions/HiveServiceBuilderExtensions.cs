using IronHive.Providers.Anthropic;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Anthropic의 모든 서비스들을 지정된 이름으로 설정합니다.
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
