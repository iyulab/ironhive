using IronHive.Providers.Anthropic;

namespace IronHive.Abstractions;

public static class HiveServiceExtensions
{
    /// <summary>
    /// Anthropic Hive 서비스에 대한 확장 메서드를 정의합니다.
    /// </summary>
    public static IHiveService SetAnthropicProviders(
        this IHiveService service,
        string name,
        AnthropicConfig config)
    {
        service.SetModelCatalogProvider(new AnthropicModelCatalogProvider(config)
        {
            ProviderName = name
        });
        service.SetMessageGenerator(new AnthropicMessageGenerator(config)
        {
            ProviderName = name
        });
        return service;
    }

    /// <summary>
    /// 지정된 이름에 해당하는 Anthropic 서비스를 제거합니다.
    /// </summary>
    public static IHiveService RemoveAnthropicProviders(
        this IHiveService service,
        string name)
    {
        service.RemoveModelCatalogProvider(name);
        service.RemoveMessageGenerator(name);
        return service;
    }
}
