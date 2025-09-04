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
        AnthropicConfig config)
    {
        service.Providers.SetModelCatalog(providerName, new AnthropicModelCatalog(config));
        service.Providers.SetMessageGenerator(providerName, new AnthropicMessageGenerator(config));
        return service;
    }

    /// <summary>
    /// 지정된 이름에 해당하는 모든 Anthropic 서비스를 제거합니다.
    /// </summary>
    public static IHiveService RemoveAnthropicProviders(
        this IHiveService service,
        string providerName)
    {
        service.Providers.RemoveAll(providerName);
        return service;
    }
}
