using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Messages;
using IronHive.Providers.Ollama;
using IronHive.Providers.Ollama.Share;

namespace IronHive.Abstractions.Registries;

public static class ProviderRegistryExtensions
{
    /// <summary>
    /// Ollama의 모든 서비스들을 지정된 이름으로 설정합니다.
    /// </summary>
    public static void SetOllamaProviders(
        this IProviderRegistry providers,
        string providerName,
        OllamaConfig config,
        OllamaServiceType serviceType = OllamaServiceType.Models | OllamaServiceType.Chat)
    {
        if (serviceType.HasFlag(OllamaServiceType.Models))
            providers.SetModelCatalog(providerName, new OllamaModelCatalog(config));

        if (serviceType.HasFlag(OllamaServiceType.Chat))
            providers.SetMessageGenerator(providerName, new OllamaMessageGenerator(config));
    }

    /// <summary>
    /// 지정된 이름에 해당하는 모든 Ollama 서비스를 제거합니다.
    /// </summary>
    public static void RemoveOllamaProviders(
        this IProviderRegistry providers,
        string providerName,
        OllamaServiceType serviceType = OllamaServiceType.Models | OllamaServiceType.Chat)
    {
        if (serviceType.HasFlag(OllamaServiceType.Models))
            providers.Remove<IModelCatalog>(providerName);

        if (serviceType.HasFlag(OllamaServiceType.Chat))
            providers.Remove<IMessageGenerator>(providerName);
    }
}
