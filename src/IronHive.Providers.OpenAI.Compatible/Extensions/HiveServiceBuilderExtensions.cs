using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible;
using IronHive.Providers.OpenAI.Compatible.Adapters;

namespace IronHive.Abstractions;

public static class CompatibleHiveServiceBuilderExtensions
{
    #region xAI (Grok)

    /// <summary>
    /// xAI (Grok) 서비스를 추가합니다.
    /// 기본적으로 Responses API를 사용합니다.
    /// </summary>
    /// <param name="builder">HiveServiceBuilder 인스턴스</param>
    /// <param name="providerName">등록할 Provider 이름</param>
    /// <param name="apiKey">xAI API 키</param>
    /// <param name="configure">추가 설정 (검색, store 등)</param>
    /// <param name="serviceType">사용할 서비스 타입 (기본: Responses)</param>
    public static IHiveServiceBuilder AddxAI(
        this IHiveServiceBuilder builder,
        string providerName,
        string apiKey,
        Action<xAIConfig>? configure = null,
        CompatibleServiceType serviceType = CompatibleServiceType.Responses)
    {
        var config = new xAIConfig { ApiKey = apiKey };
        configure?.Invoke(config);

        if (serviceType.HasFlag(CompatibleServiceType.Responses))
        {
            // xAI는 OpenAI Responses API와 완전 호환
            builder.AddMessageGenerator(providerName, new OpenAIResponseMessageGenerator(config.ToOpenAIConfig()));
        }
        else if (serviceType.HasFlag(CompatibleServiceType.ChatCompletion))
        {
            // ChatCompletion은 Adapter를 통해 xAI 특수 기능 지원
            builder.AddMessageGenerator(providerName, new CompatibleChatMessageGenerator(config));
        }

        return builder;
    }

    #endregion

    #region Groq

    /// <summary>
    /// Groq 서비스를 추가합니다.
    /// </summary>
    /// <param name="builder">HiveServiceBuilder 인스턴스</param>
    /// <param name="providerName">등록할 Provider 이름</param>
    /// <param name="apiKey">Groq API 키</param>
    /// <param name="configure">추가 설정</param>
    /// <param name="serviceType">사용할 서비스 타입 (기본: ChatCompletion)</param>
    public static IHiveServiceBuilder AddGroq(
        this IHiveServiceBuilder builder,
        string providerName,
        string apiKey,
        Action<GroqConfig>? configure = null,
        CompatibleServiceType serviceType = CompatibleServiceType.ChatCompletion)
    {
        var config = new GroqConfig { ApiKey = apiKey };
        configure?.Invoke(config);

        if (serviceType.HasFlag(CompatibleServiceType.ChatCompletion))
        {
            builder.AddMessageGenerator(providerName, new CompatibleChatMessageGenerator(config));
        }

        return builder;
    }

    #endregion

    #region DeepSeek

    /// <summary>
    /// DeepSeek 서비스를 추가합니다.
    /// </summary>
    /// <param name="builder">HiveServiceBuilder 인스턴스</param>
    /// <param name="providerName">등록할 Provider 이름</param>
    /// <param name="apiKey">DeepSeek API 키</param>
    /// <param name="configure">추가 설정 (thinking 모드, beta API 등)</param>
    /// <param name="serviceType">사용할 서비스 타입 (기본: ChatCompletion)</param>
    public static IHiveServiceBuilder AddDeepSeek(
        this IHiveServiceBuilder builder,
        string providerName,
        string apiKey,
        Action<DeepSeekConfig>? configure = null,
        CompatibleServiceType serviceType = CompatibleServiceType.ChatCompletion)
    {
        var config = new DeepSeekConfig { ApiKey = apiKey };
        configure?.Invoke(config);

        if (serviceType.HasFlag(CompatibleServiceType.ChatCompletion))
        {
            builder.AddMessageGenerator(providerName, new CompatibleChatMessageGenerator(config));
        }

        return builder;
    }

    #endregion

    #region Together AI

    /// <summary>
    /// Together AI 서비스를 추가합니다.
    /// </summary>
    /// <param name="builder">HiveServiceBuilder 인스턴스</param>
    /// <param name="providerName">등록할 Provider 이름</param>
    /// <param name="apiKey">Together AI API 키</param>
    /// <param name="configure">추가 설정</param>
    /// <param name="serviceType">사용할 서비스 타입 (기본: ChatCompletion | Embeddings)</param>
    public static IHiveServiceBuilder AddTogetherAI(
        this IHiveServiceBuilder builder,
        string providerName,
        string apiKey,
        Action<TogetherAIConfig>? configure = null,
        CompatibleServiceType serviceType = CompatibleServiceType.ChatCompletion | CompatibleServiceType.Embeddings)
    {
        var config = new TogetherAIConfig { ApiKey = apiKey };
        configure?.Invoke(config);

        if (serviceType.HasFlag(CompatibleServiceType.ChatCompletion))
        {
            builder.AddMessageGenerator(providerName, new CompatibleChatMessageGenerator(config));
        }

        if (serviceType.HasFlag(CompatibleServiceType.Embeddings))
        {
            // TogetherAI는 OpenAI Embeddings API와 호환
            builder.AddEmbeddingGenerator(providerName, new OpenAIEmbeddingGenerator(config.ToOpenAIConfig()));
        }

        return builder;
    }

    #endregion

    #region Fireworks AI

    /// <summary>
    /// Fireworks AI 서비스를 추가합니다.
    /// </summary>
    /// <param name="builder">HiveServiceBuilder 인스턴스</param>
    /// <param name="providerName">등록할 Provider 이름</param>
    /// <param name="apiKey">Fireworks AI API 키</param>
    /// <param name="configure">추가 설정</param>
    /// <param name="serviceType">사용할 서비스 타입 (기본: ChatCompletion | Embeddings)</param>
    public static IHiveServiceBuilder AddFireworks(
        this IHiveServiceBuilder builder,
        string providerName,
        string apiKey,
        Action<FireworksConfig>? configure = null,
        CompatibleServiceType serviceType = CompatibleServiceType.ChatCompletion | CompatibleServiceType.Embeddings)
    {
        var config = new FireworksConfig { ApiKey = apiKey };
        configure?.Invoke(config);

        if (serviceType.HasFlag(CompatibleServiceType.ChatCompletion))
        {
            builder.AddMessageGenerator(providerName, new CompatibleChatMessageGenerator(config));
        }

        if (serviceType.HasFlag(CompatibleServiceType.Embeddings))
        {
            // Fireworks는 OpenAI Embeddings API와 호환
            builder.AddEmbeddingGenerator(providerName, new OpenAIEmbeddingGenerator(config.ToOpenAIConfig()));
        }

        return builder;
    }

    #endregion

    #region Perplexity

    /// <summary>
    /// Perplexity 서비스를 추가합니다.
    /// </summary>
    /// <param name="builder">HiveServiceBuilder 인스턴스</param>
    /// <param name="providerName">등록할 Provider 이름</param>
    /// <param name="apiKey">Perplexity API 키</param>
    /// <param name="configure">추가 설정</param>
    /// <param name="serviceType">사용할 서비스 타입 (기본: ChatCompletion)</param>
    public static IHiveServiceBuilder AddPerplexity(
        this IHiveServiceBuilder builder,
        string providerName,
        string apiKey,
        Action<PerplexityConfig>? configure = null,
        CompatibleServiceType serviceType = CompatibleServiceType.ChatCompletion)
    {
        var config = new PerplexityConfig { ApiKey = apiKey };
        configure?.Invoke(config);

        if (serviceType.HasFlag(CompatibleServiceType.ChatCompletion))
        {
            builder.AddMessageGenerator(providerName, new CompatibleChatMessageGenerator(config));
        }

        return builder;
    }

    #endregion

    #region OpenRouter

    /// <summary>
    /// OpenRouter 서비스를 추가합니다.
    /// </summary>
    /// <param name="builder">HiveServiceBuilder 인스턴스</param>
    /// <param name="providerName">등록할 Provider 이름</param>
    /// <param name="apiKey">OpenRouter API 키</param>
    /// <param name="configure">추가 설정 (사이트 URL, 앱 이름, transforms 등)</param>
    /// <param name="serviceType">사용할 서비스 타입 (기본: ChatCompletion)</param>
    public static IHiveServiceBuilder AddOpenRouter(
        this IHiveServiceBuilder builder,
        string providerName,
        string apiKey,
        Action<OpenRouterConfig>? configure = null,
        CompatibleServiceType serviceType = CompatibleServiceType.ChatCompletion)
    {
        var config = new OpenRouterConfig { ApiKey = apiKey };
        configure?.Invoke(config);

        if (serviceType.HasFlag(CompatibleServiceType.ChatCompletion))
        {
            builder.AddMessageGenerator(providerName, new CompatibleChatMessageGenerator(config));
        }

        return builder;
    }

    #endregion

    #region Self-hosted (vLLM, GPUStack, Custom)

    /// <summary>
    /// vLLM 서비스를 추가합니다. (Self-hosted)
    /// </summary>
    /// <param name="builder">HiveServiceBuilder 인스턴스</param>
    /// <param name="providerName">등록할 Provider 이름</param>
    /// <param name="baseUrl">vLLM 서버 URL (예: http://localhost:8000/v1)</param>
    /// <param name="apiKey">API 키 (선택사항, 서버 설정에 따라 필요)</param>
    /// <param name="serviceType">사용할 서비스 타입 (기본: ChatCompletion)</param>
    public static IHiveServiceBuilder AddvLLM(
        this IHiveServiceBuilder builder,
        string providerName,
        string baseUrl,
        string? apiKey = null,
        CompatibleServiceType serviceType = CompatibleServiceType.ChatCompletion)
    {
        var config = new CompatibleConfig
        {
            Provider = CompatibleProvider.vLLM,
            BaseUrl = baseUrl,
            ApiKey = apiKey ?? string.Empty,
        };

        if (serviceType.HasFlag(CompatibleServiceType.ChatCompletion))
        {
            builder.AddMessageGenerator(providerName, new CompatibleChatMessageGenerator(config));
        }

        return builder;
    }

    /// <summary>
    /// GPUStack 서비스를 추가합니다. (Self-hosted)
    /// </summary>
    /// <param name="builder">HiveServiceBuilder 인스턴스</param>
    /// <param name="providerName">등록할 Provider 이름</param>
    /// <param name="baseUrl">GPUStack 서버 URL</param>
    /// <param name="apiKey">API 키</param>
    /// <param name="serviceType">사용할 서비스 타입 (기본: ChatCompletion)</param>
    public static IHiveServiceBuilder AddGPUStack(
        this IHiveServiceBuilder builder,
        string providerName,
        string baseUrl,
        string apiKey,
        CompatibleServiceType serviceType = CompatibleServiceType.ChatCompletion)
    {
        var config = new CompatibleConfig
        {
            Provider = CompatibleProvider.GPUStack,
            BaseUrl = baseUrl,
            ApiKey = apiKey,
        };

        if (serviceType.HasFlag(CompatibleServiceType.ChatCompletion))
        {
            builder.AddMessageGenerator(providerName, new CompatibleChatMessageGenerator(config));
        }

        return builder;
    }

    /// <summary>
    /// 사용자 정의 OpenAI 호환 서비스를 추가합니다.
    /// </summary>
    /// <param name="builder">HiveServiceBuilder 인스턴스</param>
    /// <param name="providerName">등록할 Provider 이름</param>
    /// <param name="baseUrl">서비스 Base URL</param>
    /// <param name="apiKey">API 키</param>
    /// <param name="serviceType">사용할 서비스 타입 (기본: ChatCompletion)</param>
    public static IHiveServiceBuilder AddCustomCompatible(
        this IHiveServiceBuilder builder,
        string providerName,
        string baseUrl,
        string apiKey,
        CompatibleServiceType serviceType = CompatibleServiceType.ChatCompletion)
    {
        var config = new CompatibleConfig
        {
            Provider = CompatibleProvider.Custom,
            BaseUrl = baseUrl,
            ApiKey = apiKey,
        };

        if (serviceType.HasFlag(CompatibleServiceType.ChatCompletion))
        {
            builder.AddMessageGenerator(providerName, new CompatibleChatMessageGenerator(config));
        }

        return builder;
    }

    #endregion
}
