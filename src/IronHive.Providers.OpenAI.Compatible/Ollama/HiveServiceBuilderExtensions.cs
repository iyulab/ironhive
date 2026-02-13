using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.Ollama;

namespace IronHive.Abstractions;

public static partial class CompatibleHiveServiceBuilderExtensions
{
    /// <summary>
    /// Ollama 서비스들을 추가합니다.
    /// </summary>
    public static IHiveServiceBuilder AddOllamaProviders(
        this IHiveServiceBuilder builder,
        string providerName,
        OllamaConfig config,
        OllamaServiceType serviceType = OllamaServiceType.All)
    {
        if (serviceType.HasFlag(OllamaServiceType.Models))
            builder.AddModelCatalog(providerName, new OpenAIModelCatalog(config.ToOpenAI()));

        if (serviceType.HasFlag(OllamaServiceType.Language))
            builder.AddMessageGenerator(providerName, new OllamaMessageGenerator(config));

        if (serviceType.HasFlag(OllamaServiceType.Embeddings))
            builder.AddEmbeddingGenerator(providerName, new OpenAIEmbeddingGenerator(config.ToOpenAI()));

        return builder;
    }
}
