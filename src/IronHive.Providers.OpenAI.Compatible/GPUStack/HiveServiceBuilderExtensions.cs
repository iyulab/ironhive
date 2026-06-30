using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.GpuStack;

namespace IronHive.Abstractions;

public static partial class CompatibleHiveServiceBuilderExtensions
{
    /// <summary>
    /// GPUStack 서비스들을 추가합니다.
    /// </summary>
    public static IHiveServiceBuilder AddGpuStackProviders(
        this IHiveServiceBuilder builder,
        string providerName,
        GpuStackConfig config,
        GpuStackServiceType serviceType = GpuStackServiceType.All)
    {
        if (serviceType.HasFlag(GpuStackServiceType.Models))
            builder.AddModelFinder(providerName, new OpenAIModelFinder(config.ToOpenAI()));

        if (serviceType.HasFlag(GpuStackServiceType.Language))
            builder.AddMessageGenerator(providerName, new GpuStackMessageGenerator(config));

        if (serviceType.HasFlag(GpuStackServiceType.Embeddings))
            builder.AddEmbeddingGenerator(providerName, new OpenAIEmbeddingGenerator(config.ToOpenAI()));

        return builder;
    }
}
