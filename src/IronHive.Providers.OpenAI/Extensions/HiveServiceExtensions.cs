using IronHive.Providers.OpenAI;

namespace IronHive.Abstractions;

public static class HiveServiceExtensions
{
    /// <summary>
    /// OpenAI Hive 서비스에 대한 확장 메서드를 정의합니다.
    /// </summary>
    public static IHiveService SetOpenAIProviders(
        this IHiveService service,
        string name,
        OpenAIConfig config)
    {
        service.SetModelCatalogProvider(new OpenAIModelCatalogProvider(config)
        {
            ProviderName = name
        });
        service.SetMessageGenerator(new OpenAIMessageGenerator(config)
        {
            ProviderName = name
        });
        service.SetEmbeddingGenerator(new OpenAIEmbeddingGenerator(config)
        {
            ProviderName = name
        });
        return service;
    }

    /// <summary>
    /// 지정된 이름에 해당하는 OpenAI 서비스를 제거합니다.
    /// </summary>
    public static IHiveService RemoveOpenAIProviders(
        this IHiveService service,
        string name)
    {
        service.RemoveModelCatalogProvider(name);
        service.RemoveMessageGenerator(name);
        service.RemoveEmbeddingGenerator(name);
        return service;
    }
}
