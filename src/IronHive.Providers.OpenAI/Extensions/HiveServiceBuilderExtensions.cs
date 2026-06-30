using IronHive.Providers.OpenAI;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// OpenAI의 모든 서비스들을 지정된 이름으로 설정합니다.
    /// </summary>
    public static IHiveServiceBuilder AddOpenAIProviders(
        this IHiveServiceBuilder builder,
        string providerName,
        OpenAIConfig config,
        OpenAIServiceType serviceType = OpenAIServiceType.All)
    {
        if (serviceType.HasFlag(OpenAIServiceType.Models))
            builder.AddModelFinder(providerName, new OpenAIModelFinder(config));

        if (serviceType.HasFlag(OpenAIServiceType.Messages))
            builder.AddMessageGenerator(providerName, new OpenAIMessageGenerator(config));

        if (serviceType.HasFlag(OpenAIServiceType.Embeddings))
            builder.AddEmbeddingGenerator(providerName, new OpenAIEmbeddingGenerator(config));

        if (serviceType.HasFlag(OpenAIServiceType.Images))
            builder.AddImageGenerator(providerName, new OpenAIImageGenerator(config));

        if (serviceType.HasFlag(OpenAIServiceType.Audio))
            builder.AddAudioProcessor(providerName, new OpenAIAudioProcessor(config));

        return builder;
    }
}
