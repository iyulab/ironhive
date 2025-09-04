using IronHive.Providers.Ollama;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Ollama의 모든 서비스들을 지정된 이름으로 설정합니다.
    /// </summary>
    public static IHiveServiceBuilder AddOllamaProviders(
        this IHiveServiceBuilder builder,
        string providerName,
        OllamaConfig config)
    {
        builder.AddModelCatalog(providerName, new OllamaModelCatalog(config));
        builder.AddMessageGenerator(providerName, new OllamaMessageGenerator(config));
        builder.AddEmbeddingGenerator(providerName, new OllamaEmbeddingGenerator(config));
        return builder;
    }
}
