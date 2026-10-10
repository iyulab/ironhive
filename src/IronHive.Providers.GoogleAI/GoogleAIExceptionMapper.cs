using System.Globalization;
using System.Text.RegularExpressions;
using Google.GenAI;
using IronHive.Abstractions.Exceptions;

namespace IronHive.Providers.GoogleAI;

/// <summary>
/// Normalizes Gemini API errors to IronHive domain exceptions. Currently covers
/// context-window overflow (HTTP 400, <c>status == "INVALID_ARGUMENT"</c>, message "The input
/// token count (X) exceeds the maximum number of tokens allowed (Y).") ->
/// <see cref="ContextOverflowException"/>, a refusal the account cannot pay (402, or 429 <c>insufficient_quota</c>) ->
/// <see cref="BillingException"/>, and any other 429 -> <see cref="RateLimitException"/>. The Gemini <c>status</c>
/// (<c>RESOURCE_EXHAUSTED</c>, <c>INVALID_ARGUMENT</c>) is the exception's <see cref="HiveException.ErrorCode"/>.
/// </summary>
internal static partial class GoogleAIExceptionMapper
{
    /// <summary>Returns the normalized exception when <paramref name="exception"/> matches a
    /// known error shape; otherwise null (leaving the original exception to propagate).</summary>
    public static Exception? Map(Exception exception, CancellationToken cancellationToken = default)
        => Map(exception, keyUsesRetiredFormat: false, cancellationToken);

    /// <inheritdoc cref="Map(Exception, CancellationToken)"/>
    /// <param name="keyUsesRetiredFormat">
    /// Whether the configured API key is in the format Gemini retired in 2026-09 — the caller knows this,
    /// the mapper cannot, and it decides whether a 401/403 carries the re-issue hint.
    /// </param>
    public static Exception? Map(
        Exception exception,
        bool keyUsesRetiredFormat,
        CancellationToken cancellationToken = default)
    {
        if (IsTimeout(exception, cancellationToken, out var timeout))
            return timeout;

        if (IsRetiredKeyFormatRejected(exception, keyUsesRetiredFormat, out var retiredKey))
            return retiredKey;

        if (IsContextOverflow(exception, out var overflow))
            return overflow;

        if (IsBilling(exception, out var billing))
            return billing;

        if (IsRateLimit(exception, out var rateLimit))
            return rateLimit;

        return null;
    }

    /// <summary>
    /// The SDK's own request timeout cancels the request via an internal
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

        result = new TimeoutException("The Gemini request timed out.", exception);
        return true;
    }

    /// <summary>
    /// Gemini stopped accepting the long-standing <c>AIza</c> key format in 2026-09, and a key in that
    /// format now fails as an ordinary 401/403 — which reads as "wrong key" and sends the caller looking
    /// for a typo in a key that is correct, just retired. Only raised when the configured key is in that
    /// format: with a re-issued key a 401/403 is an ordinary authentication failure and is left alone.
    /// </summary>
    private static bool IsRetiredKeyFormatRejected(
        Exception exception, bool keyUsesRetiredFormat, out HiveException? result)
    {
        result = null;
        if (!keyUsesRetiredFormat || exception is not ClientError { StatusCode: 401 or 403 } rejected)
            return false;

        result = new HiveException(
            $"{rejected.Message} The configured API key is in the retired key format, which Gemini stopped " +
            "accepting in September 2026; re-issue the key in Google AI Studio. The library does not check " +
            "the key format, so this is a hint rather than a verdict.",
            rejected);
        return true;
    }

    [GeneratedRegex(@"input token count \((\d+)\) exceeds the maximum number of tokens allowed \((\d+)\)", RegexOptions.IgnoreCase)]
    private static partial Regex TokenCountExceededPattern();

    private static bool IsContextOverflow(Exception exception, out ContextOverflowException? result)
    {
        result = null;
        if (exception is not ClientError { StatusCode: 400, Status: "INVALID_ARGUMENT" } clientError)
            return false;

        var message = clientError.Message;
        if (!message.Contains("exceeds the maximum number of tokens allowed", StringComparison.OrdinalIgnoreCase))
            return false;

        int? contextWindow = null;
        int? requestTokens = null;
        if (TokenCountExceededPattern().Match(message) is { Success: true } match)
        {
            if (int.TryParse(match.Groups[1].Value, out var requested))
                requestTokens = requested;
            if (int.TryParse(match.Groups[2].Value, out var window))
                contextWindow = window;
        }

        result = new ContextOverflowException(message, clientError)
        {
            ContextWindow = contextWindow,
            RequestTokens = requestTokens,
            ErrorCode = clientError.Status,
        };
        return true;
    }

    /// <summary>
    /// The account cannot pay: HTTP 402 Payment Required (a metering gateway in front of the Gemini API refusing an
    /// unfunded account), or a 429 whose body says <c>insufficient_quota</c> — the code an OpenAI-style gateway uses for an
    /// exhausted balance. Checked before the rate limit: waiting does not clear either.
    /// </summary>
    private static bool IsBilling(Exception exception, out BillingException? result)
    {
        result = exception switch
        {
            ClientError { StatusCode: 402 } paymentRequired => new BillingException(paymentRequired.Message, paymentRequired)
            {
                StatusCode = System.Net.HttpStatusCode.PaymentRequired,
                ErrorCode = paymentRequired.Status,
            },
            ClientError { StatusCode: 429 } quota when quota.Message.Contains("insufficient_quota", StringComparison.OrdinalIgnoreCase)
                => new BillingException(quota.Message, quota)
                {
                    StatusCode = System.Net.HttpStatusCode.TooManyRequests,
                    ErrorCode = "insufficient_quota",
                },
            _ => null,
        };
        return result is not null;
    }

    [GeneratedRegex(@"""retryDelay""\s*:\s*""(\d+(?:\.\d+)?)s""", RegexOptions.IgnoreCase)]
    private static partial Regex RetryDelayPattern();

    private static bool IsRateLimit(Exception exception, out RateLimitException? result)
    {
        result = null;
        // Any 429 is a rate limit: Gemini says RESOURCE_EXHAUSTED, a gateway in front of it may say something else or
        // nothing — the status is the signal. A billing refusal sent as 429 was taken by IsBilling first.
        if (exception is not ClientError { StatusCode: 429 } rateLimited)
            return false;

        // The SDK doesn't expose a dedicated RetryInfo/retryDelay field, so this falls back
        // to scraping it out of the error message on the off chance the SDK preserved the
        // raw error body there; if not, RetryAfter is left null.
        TimeSpan? retryAfter = null;
        if (RetryDelayPattern().Match(rateLimited.Message) is { Success: true } match
            && double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
        {
            retryAfter = TimeSpan.FromSeconds(seconds);
        }

        result = new RateLimitException(rateLimited.Message, rateLimited) { RetryAfter = retryAfter, ErrorCode = rateLimited.Status };
        return true;
    }
}
