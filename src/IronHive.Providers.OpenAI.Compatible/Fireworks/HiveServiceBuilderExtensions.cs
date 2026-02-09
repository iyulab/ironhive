using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.Fireworks;

namespace IronHive.Abstractions;

public static partial class CompatibleHiveServiceBuilderExtensions
{
    /// <summary>
    /// Fireworks AI 서비스들을 추가합니다.
    /// </summary>
    public static IHiveServiceBuilder AddFireworksProviders(
        this IHiveServiceBuilder builder,
        string providerName,
        FireworksConfig config,
        FireworksServiceType serviceType = FireworksServiceType.All)
    {
        if (serviceType.HasFlag(FireworksServiceType.Models))
            builder.AddModelCatalog(providerName, new OpenAIModelCatalog(config.ToOpenAI()));

        if (serviceType.HasFlag(FireworksServiceType.Language))
            builder.AddMessageGenerator(providerName, new FireworksMessageGenerator(config));

        if (serviceType.HasFlag(FireworksServiceType.Embeddings))
            builder.AddEmbeddingGenerator(providerName, new OpenAIEmbeddingGenerator(config.ToOpenAI()));

        return builder;
    }
}
