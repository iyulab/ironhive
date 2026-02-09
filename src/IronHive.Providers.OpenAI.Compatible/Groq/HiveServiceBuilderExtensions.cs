using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.Groq;

namespace IronHive.Abstractions;

public static partial class CompatibleHiveServiceBuilderExtensions
{
    /// <summary>
    /// Groq 서비스들을 추가합니다.
    /// </summary>
    public static IHiveServiceBuilder AddGroqProviders(
        this IHiveServiceBuilder builder,
        string providerName,
        GroqConfig config,
        GroqServiceType serviceType = GroqServiceType.All)
    {
        if (serviceType.HasFlag(GroqServiceType.Models))
            builder.AddModelCatalog(providerName, new OpenAIModelCatalog(config.ToOpenAI()));

        if (serviceType.HasFlag(GroqServiceType.Language))
            builder.AddMessageGenerator(providerName, new GroqMessageGenerator(config));

        return builder;
    }
}
