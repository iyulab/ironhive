namespace IronHive.Providers.OpenAI.Compatible.Adapters;

/// <summary>
/// Provider 타입에 맞는 어댑터를 생성하는 팩토리입니다.
/// </summary>
public static class ProviderAdapterFactory
{
    private static readonly Dictionary<CompatibleProvider, IProviderAdapter> _adapters = new()
    {
        { CompatibleProvider.xAI, new xAIAdapter() },
        { CompatibleProvider.Groq, new GroqAdapter() },
        { CompatibleProvider.DeepSeek, new DeepSeekAdapter() },
        { CompatibleProvider.TogetherAI, new TogetherAIAdapter() },
        { CompatibleProvider.Fireworks, new FireworksAdapter() },
        { CompatibleProvider.Perplexity, new PerplexityAdapter() },
        { CompatibleProvider.OpenRouter, new OpenRouterAdapter() },
        { CompatibleProvider.vLLM, new DefaultAdapter(CompatibleProvider.vLLM) },
        { CompatibleProvider.GPUStack, new DefaultAdapter(CompatibleProvider.GPUStack) },
        { CompatibleProvider.Custom, new DefaultAdapter(CompatibleProvider.Custom) },
    };

    /// <summary>
    /// 지정된 Provider에 맞는 어댑터를 반환합니다.
    /// </summary>
    public static IProviderAdapter GetAdapter(CompatibleProvider provider)
    {
        if (_adapters.TryGetValue(provider, out var adapter))
        {
            return adapter;
        }

        return new DefaultAdapter(provider);
    }

    /// <summary>
    /// Config에서 Provider를 추출하여 적절한 어댑터를 반환합니다.
    /// </summary>
    public static IProviderAdapter GetAdapter(CompatibleConfig config)
    {
        return GetAdapter(config.Provider);
    }
}
