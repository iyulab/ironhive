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
        string providerName,
        OpenAIConfig config)
    {
        service.Providers.Set<IModelCatalog, OpenAIModelCatalog>(
            providerName, new OpenAIModelCatalog(config));
        service.Providers.Set<IMessageGenerator, OpenAIMessageGenerator>(
            providerName, new OpenAIMessageGenerator(config));
        service.Providers.Set<IEmbeddingGenerator, OpenAIEmbeddingGenerator>(
            providerName, new OpenAIEmbeddingGenerator(config));
        return service;
    }

    /// <summary>
    /// 지정된 이름에 해당하는 OpenAI 서비스를 제거합니다.
    /// </summary>
    public static IHiveService RemoveOpenAIProviders(
        this IHiveService service,
        string providerName)
    {
        service.Providers.Remove<IModelCatalog>(providerName);
        service.Providers.Remove<IMessageGenerator>(providerName);
        service.Providers.Remove<IEmbeddingGenerator>(providerName);
        return service;
    }
}
