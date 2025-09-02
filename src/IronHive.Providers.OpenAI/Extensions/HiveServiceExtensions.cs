using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Messages;
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
        service.Providers.Set<IModelCatalogProvider>(new OpenAIModelCatalogProvider(config)
        {
            ProviderName = name
        });
        service.Providers.Set<IMessageGenerator>(new OpenAIMessageGenerator(config)
        {
            ProviderName = name
        });
        service.Providers.Set<IEmbeddingGenerator>(new OpenAIEmbeddingGenerator(config)
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
        service.Providers.Remove<IModelCatalogProvider>(name);
        service.Providers.Remove<IMessageGenerator>(name);
        service.Providers.Remove<IEmbeddingGenerator>(name);
        return service;
    }
}
