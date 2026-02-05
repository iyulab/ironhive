using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible;
using IronHive.Providers.OpenAI.Compatible.XAI;
using IronHive.Providers.OpenAI.Compatible.Groq;
using IronHive.Providers.OpenAI.Compatible.DeepSeek;
using IronHive.Providers.OpenAI.Compatible.TogetherAI;
using IronHive.Providers.OpenAI.Compatible.Fireworks;
using IronHive.Providers.OpenAI.Compatible.Perplexity;
using IronHive.Providers.OpenAI.Compatible.OpenRouter;

namespace IronHive.Abstractions;

public static class CompatibleHiveServiceBuilderExtensions
{
    /// <summary>
    /// xAI (Grok) 서비스를 추가합니다.
    /// 기본적으로 Responses API를 사용합니다.
    /// </summary>
    public static IHiveServiceBuilder AddxAIProvider(
        this IHiveServiceBuilder builder,
        string providerName,
        XAIConfig config)
    {
        builder.AddMessageGenerator(providerName, new XAIMessageGenerator(config));
        return builder;
    }

    /// <summary>
    /// Groq 서비스를 추가합니다.
    /// </summary>
    public static IHiveServiceBuilder AddGroqProvider(
        this IHiveServiceBuilder builder,
        string providerName,
        GroqConfig config)
    {
        builder.AddMessageGenerator(providerName, new GroqMessageGenerator(config));
        return builder;
    }

    /// <summary>
    /// DeepSeek 서비스를 추가합니다.
    /// </summary>
    public static IHiveServiceBuilder AddDeepSeekProvider(
        this IHiveServiceBuilder builder,
        string providerName,
        DeepSeekConfig config)
    {
        builder.AddMessageGenerator(providerName, new DeepSeekMessageGenerator(config));
        return builder;
    }

    /// <summary>
    /// Together AI 서비스를 추가합니다.
    /// </summary>
    public static IHiveServiceBuilder AddTogetherAIProvider(
        this IHiveServiceBuilder builder,
        string providerName,
        TogetherAIConfig config)
    {
        builder.AddMessageGenerator(providerName, new TogetherAIMessageGenerator(config));
        return builder;
    }

    /// <summary>
    /// Fireworks AI 서비스를 추가합니다.
    /// </summary>
    public static IHiveServiceBuilder AddFireworksProvider(
        this IHiveServiceBuilder builder,
        string providerName,
        FireworksConfig config)
    {
        builder.AddMessageGenerator(providerName, new FireworksMessageGenerator(config));
        return builder;
    }

    /// <summary>
    /// Perplexity 서비스를 추가합니다.
    /// </summary>
    public static IHiveServiceBuilder AddPerplexityProvider(
        this IHiveServiceBuilder builder,
        string providerName,
        PerplexityConfig config)
    {
        builder.AddMessageGenerator(providerName, new PerplexityMessageGenerator(config));
        return builder;
    }

    /// <summary>
    /// OpenRouter 서비스를 추가합니다.
    /// </summary>
    public static IHiveServiceBuilder AddOpenRouterProvider(
        this IHiveServiceBuilder builder,
        string providerName,
        OpenRouterConfig config)
    {
        builder.AddMessageGenerator(providerName, new OpenRouterMessageGenerator(config));
        return builder;
    }

    /// <summary>
    /// Self-hosted 서비스들(vLLM, GPUStack, etc.)과 호환되는 OpenAI 프로바이더를 추가합니다.
    /// </summary>
    public static IHiveServiceBuilder AddCompatibleProvider(
        this IHiveServiceBuilder builder,
        string providerName,
        CompatibleConfig config)
    {
        var openAIConfig = config.ToOpenAI();
        builder.AddOpenAIProviders(providerName, openAIConfig);
        return builder;
    }
}
