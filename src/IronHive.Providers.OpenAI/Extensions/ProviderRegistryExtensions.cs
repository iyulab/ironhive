using IronHive.Abstractions.Audio;
using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Images;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Videos;
using IronHive.Providers.OpenAI;

namespace IronHive.Abstractions.Registries;

public static class ProviderRegistryExtensions
{
    /// <summary>
    /// OpenAI의 모든 서비스들을 지정된 이름으로 설정합니다.
    /// </summary>
    public static void SetOpenAIProviders(
        this IProviderRegistry providers,
        string providerName,
        OpenAIConfig config,
        OpenAIServiceType serviceType = OpenAIServiceType.All)
    {
        if (serviceType.HasFlag(OpenAIServiceType.ChatCompletion) && serviceType.HasFlag(OpenAIServiceType.Responses))
            throw new ArgumentException("ChatCompletion and Responses cannot be enabled at the same time.", nameof(serviceType));

        if (serviceType.HasFlag(OpenAIServiceType.Models))
            providers.SetModelCatalog(providerName, new OpenAIModelCatalog(config));

        if (serviceType.HasFlag(OpenAIServiceType.ChatCompletion))
            providers.SetMessageGenerator(providerName, new OpenAIChatMessageGenerator(config));

        if (serviceType.HasFlag(OpenAIServiceType.Responses))
            providers.SetMessageGenerator(providerName, new OpenAIResponseMessageGenerator(config));

        if (serviceType.HasFlag(OpenAIServiceType.Embeddings))
            providers.SetEmbeddingGenerator(providerName, new OpenAIEmbeddingGenerator(config));

        if (serviceType.HasFlag(OpenAIServiceType.Images))
            providers.SetImageGenerator(providerName, new OpenAIImageGenerator(config));

        if (serviceType.HasFlag(OpenAIServiceType.Videos))
            providers.SetVideoGenerator(providerName, new OpenAIVideoGenerator(config));

        if (serviceType.HasFlag(OpenAIServiceType.Audio))
            providers.SetAudioProcessor(providerName, new OpenAIAudioProcessor(config));
    }

    /// <summary>
    /// 지정된 이름에 해당하는 모든 OpenAI 서비스를 제거합니다.
    /// </summary>
    public static void RemoveOpenAIProviders(
        this IProviderRegistry providers,
        string providerName,
        OpenAIServiceType serviceType = OpenAIServiceType.All)
    {
        if (serviceType.HasFlag(OpenAIServiceType.Models))
            providers.Remove<IModelCatalog>(providerName);

        if (serviceType.HasFlag(OpenAIServiceType.ChatCompletion) || serviceType.HasFlag(OpenAIServiceType.Responses))
            providers.Remove<IMessageGenerator>(providerName);

        if (serviceType.HasFlag(OpenAIServiceType.Embeddings))
            providers.Remove<IEmbeddingGenerator>(providerName);

        if (serviceType.HasFlag(OpenAIServiceType.Images))
            providers.Remove<IImageGenerator>(providerName);

        if (serviceType.HasFlag(OpenAIServiceType.Videos))
            providers.Remove<IVideoGenerator>(providerName);

        if (serviceType.HasFlag(OpenAIServiceType.Audio))
            providers.Remove<IAudioProcessor>(providerName);
    }
}