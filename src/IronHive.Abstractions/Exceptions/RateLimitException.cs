namespace IronHive.Abstractions.Exceptions;

/// <summary>
/// The request was rejected because a rate limit was exceeded. Providers normalize their
/// vendor-specific rate-limit errors (e.g. OpenAI/vLLM HTTP 429 with error.type
/// <c>rate_limit_exceeded</c>, Anthropic <c>rate_limit_error</c>, Gemini HTTP 429
/// <c>RESOURCE_EXHAUSTED</c>) to this type so consumers can detect and recover
/// (backoff, retry, re-route) without string parsing. An exhausted balance or quota is not a rate
/// limit even when it arrives as HTTP 429 (OpenAI <c>insufficient_quota</c>) — waiting does not clear
/// it — and is <see cref="BillingException"/>.
/// </summary>
public class RateLimitException : HiveException
{
    public RateLimitException(string message, Exception? inner = null)
        : base(message, inner)
    { }

    /// <summary>How long to wait before retrying, when the provider exposes it (e.g. a
    /// <c>retry-after</c> response header or a <c>RetryInfo.retryDelay</c> error detail).
    /// Null when the provider's SDK surfaces the error without that information.</summary>
    public TimeSpan? RetryAfter { get; set; }
}
