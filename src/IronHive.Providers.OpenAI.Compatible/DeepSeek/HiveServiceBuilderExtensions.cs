using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.DeepSeek;

namespace IronHive.Abstractions;

public static partial class CompatibleHiveServiceBuilderExtensions
{
    /// <summary>
    /// DeepSeek 서비스들을 추가합니다.
    /// </summary>
    public static IHiveServiceBuilder AddDeepSeekProviders(
        this IHiveServiceBuilder builder,
        string providerName,
        DeepSeekConfig config,
        DeepSeekServiceType serviceType = DeepSeekServiceType.All)
    {
        if (serviceType.HasFlag(DeepSeekServiceType.Models))
            builder.AddModelCatalog(providerName, new OpenAIModelCatalog(config.ToOpenAI()));

        if (serviceType.HasFlag(DeepSeekServiceType.Language))
            builder.AddMessageGenerator(providerName, new DeepSeekMessageGenerator(config));

        return builder;
    }
}
