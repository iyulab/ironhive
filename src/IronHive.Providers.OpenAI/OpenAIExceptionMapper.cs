using System.ClientModel;
using IronHive.Abstractions.Exceptions;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// Normalizes OpenAI SDK errors to IronHive domain exceptions. Currently covers Responses API
/// context-window overflow (<c>error.code == "context_length_exceeded"</c>, which the SDK embeds
/// directly into <see cref="ClientResultException"/>'s <c>Message</c>) -> <see cref="ContextOverflowException"/>.
/// <para>
/// Unlike the legacy Chat Completions format ("This model's maximum context length is X
/// tokens..."), the Responses API's overflow message ("Your input exceeds the context window
/// of this model. Please adjust your input and try again.") carries no token counts.
/// <see cref="ContextOverflowException.ContextWindow"/> is therefore expected to be null in
/// practice here — the legacy-format regex is kept only as a best-effort fallback.
/// </para>
/// <para>The recognition itself is <see cref="OpenAIErrors"/>, public so a caller with its own SDK client
/// gets the same mapping.</para>
/// </summary>
internal static class OpenAIExceptionMapper
{
    /// <summary>Returns the normalized exception when <paramref name="exception"/> matches a
    /// known error shape; otherwise null (leaving the original exception to propagate).</summary>
    public static Exception? Map(Exception exception, CancellationToken cancellationToken = default)
    {
        if (IsTimeout(exception, cancellationToken, out var timeout))
            return timeout;

        if (IsContextOverflow(exception, out var overflow))
            return overflow;

        if (IsRateLimit(exception, out var rateLimit))
            return rateLimit;

        return null;
    }

    /// <summary>
    /// The SDK's own <c>NetworkTimeout</c> cancels the request via an internal
    /// <see cref="CancellationTokenSource"/> unrelated to the caller's, which surfaces as a
    /// bare <see cref="OperationCanceledException"/> — indistinguishable from the caller's own
    /// <paramref name="cancellationToken"/> being canceled unless checked here. Only when that
    /// token was NOT the source is this really a timeout.
    /// </summary>
    private static bool IsTimeout(Exception exception, CancellationToken cancellationToken, out TimeoutException? result)
    {
        result = null;
        if (exception is not OperationCanceledException || cancellationToken.IsCancellationRequested)
            return false;

        result = new TimeoutException("The OpenAI request timed out.", exception);
        return true;
    }

    private static bool IsContextOverflow(Exception exception, out ContextOverflowException? result)
    {
        result = OpenAIErrors.TryMapContextOverflow(exception);
        return result is not null;
    }

    private static bool IsRateLimit(Exception exception, out RateLimitException? result)
    {
        result = OpenAIErrors.TryMapRateLimit(exception);
        return result is not null;
    }
}
