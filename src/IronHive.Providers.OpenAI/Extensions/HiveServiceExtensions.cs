using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Messages;
using IronHive.Providers.OpenAI;

namespace IronHive.Abstractions;

public static class HiveServiceExtensions
{
    /// <summary>
    /// OpenAI의 모든 서비스들을 지정된 이름으로 설정합니다.
    /// </summary>
    public static IHiveService SetOpenAIProviders(
        this IHiveService service,
        string providerName,
        OpenAIConfig config,
        OpenAIServiceType serviceType = OpenAIServiceType.All)
    {
        if (serviceType.HasFlag(OpenAIServiceType.ChatCompletion) && serviceType.HasFlag(OpenAIServiceType.Responses))
            throw new ArgumentException("ChatCompletion and Responses cannot be enabled at the same time.");

        if (serviceType.HasFlag(OpenAIServiceType.Models))
            service.Providers.SetModelCatalog(providerName, new OpenAIModelCatalog(config));

        if (serviceType.HasFlag(OpenAIServiceType.ChatCompletion))
            service.Providers.SetMessageGenerator(providerName, new OpenAIMessageGenerator(config));
        
        if (serviceType.HasFlag(OpenAIServiceType.Embeddings))
            service.Providers.SetEmbeddingGenerator(providerName, new OpenAIEmbeddingGenerator(config));
        
        return service;
    }

    /// <summary>
    /// 지정된 이름에 해당하는 모든 OpenAI 서비스를 제거합니다.
    /// </summary>
    public static IHiveService RemoveOpenAIProviders(
        this IHiveService service,
        string providerName,
        OpenAIServiceType serviceType = OpenAIServiceType.All)
    {
        if (serviceType.HasFlag(OpenAIServiceType.Models))
            service.Providers.Remove<IModelCatalog>(providerName);

        if (serviceType.HasFlag(OpenAIServiceType.ChatCompletion) || serviceType.HasFlag(OpenAIServiceType.Responses))
            service.Providers.Remove<IMessageGenerator>(providerName);

        if (serviceType.HasFlag(OpenAIServiceType.Embeddings))
            service.Providers.Remove<IEmbeddingGenerator>(providerName);

        return service;
    }
}
