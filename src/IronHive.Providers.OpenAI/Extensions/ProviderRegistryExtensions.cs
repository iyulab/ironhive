using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Messages;
using IronHive.Providers.OpenAI;

namespace IronHive.Abstractions.Registries;

public static class ProviderRegistryExtensions
{
    /// <summary>
    /// OpenAI의 모든 서비스들을 지정된 이름으로 설정합니다.
    /// </summary>
    public static void SetOpenAIProviders(
        this IProviderRegistry providers,
        string providerName,
        OpenAIConfig config,
        OpenAIServiceType serviceType = OpenAIServiceType.All)
    {
        if (serviceType.HasFlag(OpenAIServiceType.ChatCompletion) && serviceType.HasFlag(OpenAIServiceType.Responses))
            throw new ArgumentException("ChatCompletion and Responses cannot be enabled at the same time.");

        if (serviceType.HasFlag(OpenAIServiceType.Models))
            providers.SetModelCatalog(providerName, new OpenAIModelCatalog(config));

        if (serviceType.HasFlag(OpenAIServiceType.ChatCompletion))
            providers.SetMessageGenerator(providerName, new OpenAIChatMessageGenerator(config));
        
        if (serviceType.HasFlag(OpenAIServiceType.Embeddings))
            providers.SetEmbeddingGenerator(providerName, new OpenAIEmbeddingGenerator(config));

        if (serviceType.HasFlag(OpenAIServiceType.Responses))
            providers.SetMessageGenerator(providerName, new OpenAIResponseMessageGenerator(config));
    }

    /// <summary>
    /// 지정된 이름에 해당하는 모든 OpenAI 서비스를 제거합니다.
    /// </summary>
    public static void RemoveOpenAIProviders(
        this IProviderRegistry providers,
        string providerName,
        OpenAIServiceType serviceType = OpenAIServiceType.All)
    {
        if (serviceType.HasFlag(OpenAIServiceType.Models))
            providers.Remove<IModelCatalog>(providerName);

        if (serviceType.HasFlag(OpenAIServiceType.ChatCompletion) || serviceType.HasFlag(OpenAIServiceType.Responses))
            providers.Remove<IMessageGenerator>(providerName);

        if (serviceType.HasFlag(OpenAIServiceType.Embeddings))
            providers.Remove<IEmbeddingGenerator>(providerName);
    }
}