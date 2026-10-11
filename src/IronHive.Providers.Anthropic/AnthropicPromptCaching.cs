namespace IronHive.Providers.Anthropic;

/// <summary>
/// Prompt caching on the Messages API. Anthropic caches a prompt prefix only where the request marks a breakpoint; with
/// automatic caching (a top-level <c>cache_control</c>) the API places it on the last cacheable block and moves it
/// forward as the conversation grows, so the next request that repeats the prefix — the next tool round, the next turn —
/// reads it at the cache-read rate instead of the input rate.
/// </summary>
/// <remarks>
/// A cache write costs more than plain input (1.25× for five minutes, 2× for one hour) and a read costs 0.1×, so caching
/// pays back as soon as one later request within the lifetime repeats the prefix — the shape of every tool loop and
/// multi-turn conversation. Prompts shorter than the model's minimum (512–4,096 tokens) are processed without caching,
/// and the usage then reports no cache tokens. The usage reports what was read and written
/// (<c>MessageTokenUsage.CachedInputTokens</c>, <c>CacheWriteInputTokens</c>, <c>CacheWritesByTtl</c>).
/// </remarks>
public enum AnthropicPromptCaching
{
    /// <summary>Automatic caching with the five-minute lifetime, refreshed on each hit (the default).</summary>
    FiveMinutes,

    /// <summary>Automatic caching with the one-hour lifetime — for prefixes reused less often than every few minutes.</summary>
    OneHour,

    /// <summary>No cache breakpoint is sent; every request pays the input rate for its whole prompt.</summary>
    Off,
}
