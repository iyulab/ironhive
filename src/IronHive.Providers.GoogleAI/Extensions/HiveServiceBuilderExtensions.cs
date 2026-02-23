using IronHive.Providers.GoogleAI;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// GoogleAI의 모든 서비스들을 지정된 이름으로 설정합니다.
    /// </summary>
    public static IHiveServiceBuilder AddGoogleAIProviders(
        this IHiveServiceBuilder builder,
        string providerName, 
        GoogleAIConfig config,
        GoogleAIServiceType serviceType = GoogleAIServiceType.All)
    {
        if (serviceType.HasFlag(GoogleAIServiceType.Models))
            builder.AddModelCatalog(providerName, new GoogleAIModelCatalog(config));

        if (serviceType.HasFlag(GoogleAIServiceType.Messages))
            builder.AddMessageGenerator(providerName, new GoogleAIMessageGenerator(config));

        if (serviceType.HasFlag(GoogleAIServiceType.Embeddings))
            builder.AddEmbeddingGenerator(providerName, new GoogleAIEmbeddingGenerator(config));

        if (serviceType.HasFlag(GoogleAIServiceType.Images))
            builder.AddImageGenerator(providerName, new GoogleAIImageGenerator(config));

        return builder;
    }
}