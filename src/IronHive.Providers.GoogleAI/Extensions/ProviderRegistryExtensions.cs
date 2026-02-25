using IronHive.Abstractions.Audio;
using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Images;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Videos;
using IronHive.Providers.GoogleAI;

namespace IronHive.Abstractions.Registries;

public static class ProviderRegistryExtensions
{
    /// <summary>
    /// GoogleAI의 모든 서비스들을 지정된 이름으로 설정합니다.
    /// </summary>
    public static void SetGoogleAIProviders(
        this IProviderRegistry providers,
        string providerName,
        GoogleAIConfig config,
        GoogleAIServiceType serviceType = GoogleAIServiceType.All)
    {
        if (serviceType.HasFlag(GoogleAIServiceType.Models))
            providers.SetModelCatalog(providerName, new GoogleAIModelCatalog(config));

        if (serviceType.HasFlag(GoogleAIServiceType.Messages))
            providers.SetMessageGenerator(providerName, new GoogleAIMessageGenerator(config));

        if (serviceType.HasFlag(GoogleAIServiceType.Embeddings))
            providers.SetEmbeddingGenerator(providerName, new GoogleAIEmbeddingGenerator(config));

        if (serviceType.HasFlag(GoogleAIServiceType.Images))
            providers.SetImageGenerator(providerName, new GoogleAIImageGenerator(config));

        if (serviceType.HasFlag(GoogleAIServiceType.Videos))
            providers.SetVideoGenerator(providerName, new GoogleAIVideoGenerator(config));

        if (serviceType.HasFlag(GoogleAIServiceType.Audio))
            providers.SetAudioProcessor(providerName, new GoogleAIAudioProcessor(config));
    }

    /// <summary>
    /// VertexAI의 모든 서비스들을 지정된 이름으로 설정합니다.
    /// </summary>
    public static void SetVertexAIProviders(
        this IProviderRegistry providers,
        string providerName,
        VertexAIConfig config,
        GoogleAIServiceType serviceType = GoogleAIServiceType.All)
    {
        if (serviceType.HasFlag(GoogleAIServiceType.Models))
            providers.SetModelCatalog(providerName, new GoogleAIModelCatalog(config));

        if (serviceType.HasFlag(GoogleAIServiceType.Messages))
            providers.SetMessageGenerator(providerName, new GoogleAIMessageGenerator(config));

        if (serviceType.HasFlag(GoogleAIServiceType.Embeddings))
            providers.SetEmbeddingGenerator(providerName, new GoogleAIEmbeddingGenerator(config));

        if (serviceType.HasFlag(GoogleAIServiceType.Images))
            providers.SetImageGenerator(providerName, new GoogleAIImageGenerator(config));

        if (serviceType.HasFlag(GoogleAIServiceType.Videos))
            providers.SetVideoGenerator(providerName, new GoogleAIVideoGenerator(config));

        if (serviceType.HasFlag(GoogleAIServiceType.Audio))
            providers.SetAudioProcessor(providerName, new GoogleAIAudioProcessor(config));
    }

    /// <summary>
    /// 지정된 이름에 해당하는 모든 GoogleAI/VertexAI 서비스를 제거합니다.
    /// </summary>
    public static void RemoveGoogleAIProviders(
        this IProviderRegistry providers,
        string providerName,
        GoogleAIServiceType serviceType = GoogleAIServiceType.All)
    {
        if (serviceType.HasFlag(GoogleAIServiceType.Models))
            providers.Remove<IModelCatalog>(providerName);

        if (serviceType.HasFlag(GoogleAIServiceType.Messages))
            providers.Remove<IMessageGenerator>(providerName);

        if (serviceType.HasFlag(GoogleAIServiceType.Embeddings))
            providers.Remove<IEmbeddingGenerator>(providerName);

        if (serviceType.HasFlag(GoogleAIServiceType.Images))
            providers.Remove<IImageGenerator>(providerName);

        if (serviceType.HasFlag(GoogleAIServiceType.Videos))
            providers.Remove<IVideoGenerator>(providerName);

        if (serviceType.HasFlag(GoogleAIServiceType.Audio))
            providers.Remove<IAudioProcessor>(providerName);
    }
}