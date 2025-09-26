using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Share;

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
        OpenAIServiceType serviceType = OpenAIServiceType.ChatCompletion | OpenAIServiceType.Embeddings | OpenAIServiceType.Models)
    {
        if (serviceType.HasFlag(OpenAIServiceType.ChatCompletion) && serviceType.HasFlag(OpenAIServiceType.Responses))
            throw new ArgumentException("ChatCompletion and Responses cannot be enabled at the same time.");

        if (serviceType.HasFlag(OpenAIServiceType.Models))
            builder.AddModelCatalog(providerName, new OpenAIModelCatalog(config));

        if (serviceType.HasFlag(OpenAIServiceType.ChatCompletion))
            builder.AddMessageGenerator(providerName, new OpenAIChatMessageGenerator(config));

        if (serviceType.HasFlag(OpenAIServiceType.Responses))
            builder.AddMessageGenerator(providerName, new OpenAIResponseMessageGenerator(config));

        if (serviceType.HasFlag(OpenAIServiceType.Embeddings))
            builder.AddEmbeddingGenerator(providerName, new OpenAIEmbeddingGenerator(config));

        return builder;
    }
}
