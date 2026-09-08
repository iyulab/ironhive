using System.ClientModel;
using System.Globalization;
using System.Text.RegularExpressions;
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
/// </summary>
internal static partial class OpenAIExceptionMapper
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

    [GeneratedRegex(@"maximum context length is (\d+) tokens?", RegexOptions.IgnoreCase)]
    private static partial Regex MaxContextPattern();

    private static bool IsContextOverflow(Exception exception, out ContextOverflowException? result)
    {
        result = null;
        if (exception is not ClientResultException clientEx)
            return false;

        var message = clientEx.Message;
        if (!message.Contains("context_length_exceeded", StringComparison.OrdinalIgnoreCase) &&
            !message.Contains("maximum context length", StringComparison.OrdinalIgnoreCase))
            return false;

        int? contextWindow = null;
        if (MaxContextPattern().Match(message) is { Success: true } match
            && int.TryParse(match.Groups[1].Value, out var window))
        {
            contextWindow = window;
        }

        result = new ContextOverflowException(message, clientEx) { ContextWindow = contextWindow };
        return true;
    }

    private static bool IsRateLimit(Exception exception, out RateLimitException? result)
    {
        result = null;
        if (exception is not ClientResultException { Status: 429 } clientEx)
            return false;

        // OpenAI's other rate-limit headers (x-ratelimit-remaining-requests etc.) are only
        // meaningful relative to the caller's configured limit, so they aren't surfaced here.
        TimeSpan? retryAfter = null;
        if (clientEx.GetRawResponse()?.Headers.TryGetValue("retry-after", out var value) == true
            && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
        {
            retryAfter = TimeSpan.FromSeconds(seconds);
        }

        result = new RateLimitException(clientEx.Message, clientEx) { RetryAfter = retryAfter };
        return true;
    }
}
