using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible;

namespace IronHive.Abstractions;

public static partial class CompatibleHiveServiceBuilderExtensions
{
    /// <summary>
    /// Registers a generic OpenAI-compatible provider (Ollama, LM Studio, vLLM, llama.cpp server, etc.)
    /// that exposes the standard <c>/v1</c> API surface. A single provider type covers any such service —
    /// only <see cref="OpenAICompatibleConfig.BaseUrl"/> differs.
    /// </summary>
    public static IHiveServiceBuilder AddOpenAICompatibleProviders(
        this IHiveServiceBuilder builder,
        string providerName,
        OpenAICompatibleConfig config,
        OpenAICompatibleServiceType serviceType = OpenAICompatibleServiceType.All)
    {
        if (serviceType.HasFlag(OpenAICompatibleServiceType.Models))
            builder.AddModelCatalog(providerName, new OpenAIModelCatalog(config.ToOpenAI()));

        if (serviceType.HasFlag(OpenAICompatibleServiceType.Language))
            builder.AddMessageGenerator(providerName, new OpenAICompatibleMessageGenerator(config));

        if (serviceType.HasFlag(OpenAICompatibleServiceType.Embeddings))
            builder.AddEmbeddingGenerator(providerName, new OpenAIEmbeddingGenerator(config.ToOpenAI()));

        return builder;
    }
}
