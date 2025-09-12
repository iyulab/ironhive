using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Messages;
using IronHive.Providers.Anthropic;

namespace IronHive.Abstractions.Registries;

public static class ProviderRegistryExtensions
{
    /// <summary>
    /// Anthropic의 모든 서비스들을 지정된 이름으로 설정합니다.
    /// </summary>
    public static void SetAnthropicProviders(
        this IProviderRegistry providers,
        string providerName,
        AnthropicConfig config,
        AnthropicServiceType serviceType = AnthropicServiceType.All)
    {
        if (serviceType.HasFlag(AnthropicServiceType.Messages))
            providers.SetMessageGenerator(providerName, new AnthropicMessageGenerator(config));

        if (serviceType.HasFlag(AnthropicServiceType.Models))
            providers.SetModelCatalog(providerName, new AnthropicModelCatalog(config));
    }

    /// <summary>
    /// 지정된 이름에 해당하는 모든 Anthropic 서비스를 제거합니다.
    /// </summary>
    public static void RemoveAnthropicProviders(
        this IProviderRegistry providers,
        string providerName,
        AnthropicServiceType serviceType = AnthropicServiceType.All)
    {
        if (serviceType.HasFlag(AnthropicServiceType.Messages))
            providers.Remove<IMessageGenerator>(providerName);

        if (serviceType.HasFlag(AnthropicServiceType.Models))
            providers.Remove<IModelCatalog>(providerName);
    }
}
