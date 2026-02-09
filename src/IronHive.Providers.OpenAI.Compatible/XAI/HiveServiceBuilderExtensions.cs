using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.XAI;

namespace IronHive.Abstractions;

public static partial class CompatibleHiveServiceBuilderExtensions
{
    /// <summary>
    /// xAI (Grok) 서비스들을 추가합니다.
    /// </summary>
    public static IHiveServiceBuilder AddXAIProviders(
        this IHiveServiceBuilder builder,
        string providerName,
        XAIConfig config,
        XAIServiceType serviceType = XAIServiceType.All)
    {
        if (serviceType.HasFlag(XAIServiceType.Models))
            builder.AddModelCatalog(providerName, new OpenAIModelCatalog(config.ToOpenAI()));
        
        if (serviceType.HasFlag(XAIServiceType.Language))
            builder.AddMessageGenerator(providerName, new XAIMessageGenerator(config));
        
        return builder;
    }
}
