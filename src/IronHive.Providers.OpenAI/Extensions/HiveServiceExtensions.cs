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
        OpenAIConfig config)
    {
        service.Providers.SetModelCatalog(providerName, new OpenAIModelCatalog(config));
        service.Providers.SetMessageGenerator(providerName, new OpenAIMessageGenerator(config));
        service.Providers.SetEmbeddingGenerator(providerName, new OpenAIEmbeddingGenerator(config));
        return service;
    }

    /// <summary>
    /// 지정된 이름에 해당하는 모든 OpenAI 서비스를 제거합니다.
    /// </summary>
    public static IHiveService RemoveOpenAIProviders(
        this IHiveService service,
        string providerName)
    {
        service.Providers.RemoveAll(providerName);
        return service;
    }
}
