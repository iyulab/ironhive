using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.OpenRouter;

namespace IronHive.Abstractions;

public static partial class CompatibleHiveServiceBuilderExtensions
{
    /// <summary>
    /// OpenRouter 서비스들을 추가합니다.
    /// </summary>
    public static IHiveServiceBuilder AddOpenRouterProviders(
        this IHiveServiceBuilder builder,
        string providerName,
        OpenRouterConfig config,
        OpenRouterServiceType serviceType = OpenRouterServiceType.All)
    {
        if (serviceType.HasFlag(OpenRouterServiceType.Models))
            builder.AddModelCatalog(providerName, new OpenAIModelCatalog(config.ToOpenAI()));

        if (serviceType.HasFlag(OpenRouterServiceType.Language))
            builder.AddMessageGenerator(providerName, new OpenRouterMessageGenerator(config));

        if (serviceType.HasFlag(OpenRouterServiceType.Embedding))
            builder.AddEmbeddingGenerator(providerName, new OpenAIEmbeddingGenerator(config.ToOpenAI()));

        return builder;
    }
}
