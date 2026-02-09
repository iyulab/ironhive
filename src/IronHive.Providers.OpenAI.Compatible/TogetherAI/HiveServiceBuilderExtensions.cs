using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.TogetherAI;

namespace IronHive.Abstractions;

public static partial class CompatibleHiveServiceBuilderExtensions
{
    /// <summary>
    /// Together AI 서비스들을 추가합니다.
    /// </summary>
    public static IHiveServiceBuilder AddTogetherAIProviders(
        this IHiveServiceBuilder builder,
        string providerName,
        TogetherAIConfig config,
        TogetherAIServiceType serviceType = TogetherAIServiceType.All)
    {
        if (serviceType.HasFlag(TogetherAIServiceType.Models))
            builder.AddModelCatalog(providerName, new OpenAIModelCatalog(config.ToOpenAI()));

        if (serviceType.HasFlag(TogetherAIServiceType.Language))
            builder.AddMessageGenerator(providerName, new TogetherAIMessageGenerator(config));

        if (serviceType.HasFlag(TogetherAIServiceType.Embeddings))
            builder.AddEmbeddingGenerator(providerName, new OpenAIEmbeddingGenerator(config.ToOpenAI()));

        return builder;
    }
}
