using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Messages;
using IronHive.Providers.Anthropic;

namespace IronHive.Abstractions;

public static class HiveServiceExtensions
{
    /// <summary>
    /// Anthropic Hive 서비스에 대한 확장 메서드를 정의합니다.
    /// </summary>
    public static IHiveService SetAnthropicProviders(
        this IHiveService service,
        string providerName,
        AnthropicConfig config)
    {
        service.Providers.Set<IModelCatalog, AnthropicModelCatalog>(
            providerName, new AnthropicModelCatalog(config));
        service.Providers.Set<IMessageGenerator, AnthropicMessageGenerator>(
            providerName, new AnthropicMessageGenerator(config));
        return service;
    }

    /// <summary>
    /// 지정된 이름에 해당하는 Anthropic 서비스를 제거합니다.
    /// </summary>
    public static IHiveService RemoveAnthropicProviders(
        this IHiveService service,
        string providerName)
    {
        service.Providers.Remove<IModelCatalog>(providerName);
        service.Providers.Remove<IMessageGenerator>(providerName);
        return service;
    }
}
