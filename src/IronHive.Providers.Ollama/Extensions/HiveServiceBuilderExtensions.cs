using IronHive.Providers.Ollama;
using IronHive.Providers.Ollama.Share;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Ollama의 모든 서비스들을 지정된 이름으로 설정합니다.
    /// </summary>
    public static IHiveServiceBuilder AddOllamaProviders(
        this IHiveServiceBuilder builder,
        string providerName,
        OllamaConfig config,
        OllamaServiceType serviceType = OllamaServiceType.Chat | OllamaServiceType.Models | OllamaServiceType.Embeddings)
    {
        if (serviceType.HasFlag(OllamaServiceType.Models))
            builder.AddModelCatalog(providerName, new OllamaModelCatalog(config));

        if (serviceType.HasFlag(OllamaServiceType.Chat))
            builder.AddMessageGenerator(providerName, new OllamaMessageGenerator(config));

        if (serviceType.HasFlag(OllamaServiceType.Embeddings))
            builder.AddEmbeddingGenerator(providerName, new OllamaEmbeddingGenerator(config));
    
        return builder;
    }
}
