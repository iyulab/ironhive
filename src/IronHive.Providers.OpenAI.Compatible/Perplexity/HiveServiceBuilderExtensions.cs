using IronHive.Providers.OpenAI.Compatible.Perplexity;

namespace IronHive.Abstractions;

public static partial class CompatibleHiveServiceBuilderExtensions
{
    /// <summary>
    /// Perplexity 서비스를 추가합니다.
    /// </summary>
    public static IHiveServiceBuilder AddPerplexityProvider(
        this IHiveServiceBuilder builder,
        string providerName,
        PerplexityConfig config)
    {
        builder.AddMessageGenerator(providerName, new PerplexityMessageGenerator(config));
        return builder;
    }
}
