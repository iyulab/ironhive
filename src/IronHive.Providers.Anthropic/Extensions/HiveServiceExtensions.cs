using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Messages;
using IronHive.Providers.Anthropic;

namespace IronHive.Abstractions;

public static class HiveServiceExtensions
{
    /// <summary>
    /// Anthropic의 모든 서비스들을 지정된 이름으로 설정합니다.
    /// </summary>
    public static IHiveService SetAnthropicProviders(
        this IHiveService service,
        string providerName,
        AnthropicConfig config,
        AnthropicServiceType serviceType = AnthropicServiceType.All)
    {
        if (serviceType.HasFlag(AnthropicServiceType.Messages))
            service.Providers.SetMessageGenerator(providerName, new AnthropicMessageGenerator(config));

        if (serviceType.HasFlag(AnthropicServiceType.Models))
            service.Providers.SetModelCatalog(providerName, new AnthropicModelCatalog(config));

        return service;
    }

    /// <summary>
    /// 지정된 이름에 해당하는 모든 Anthropic 서비스를 제거합니다.
    /// </summary>
    public static IHiveService RemoveAnthropicProviders(
        this IHiveService service,
        string providerName,
        AnthropicServiceType serviceType = AnthropicServiceType.All)
    {
        if (serviceType.HasFlag(AnthropicServiceType.Messages))
            service.Providers.Remove<IMessageGenerator>(providerName);

        if (serviceType.HasFlag(AnthropicServiceType.Models))
            service.Providers.Remove<IModelCatalog>(providerName);

        return service;
    }
}
