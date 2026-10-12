using IronHive.Abstractions.Extensions;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Google.GenAI;
using IronHive.Abstractions.Exceptions;

namespace IronHive.Providers.GoogleAI;

/// <summary>
/// Normalizes Gemini API errors to IronHive domain exceptions. Currently covers
/// context-window overflow (HTTP 400, <c>status == "INVALID_ARGUMENT"</c>, message "The input
/// token count (X) exceeds the maximum number of tokens allowed (Y).") ->
/// <see cref="ContextOverflowException"/>, a refusal the account cannot pay (402, or 429 <c>insufficient_quota</c>) ->
/// <see cref="BillingException"/>, any other 429 -> <see cref="RateLimitException"/>, and any other HTTP error (an invalid
/// key, an unknown model, a server error) -> <see cref="ProviderHttpException"/>. The exception's error code is the
/// machine-readable reason Google puts in the error's <c>google.rpc.ErrorInfo</c> detail (<c>API_KEY_INVALID</c>,
/// <c>RATE_LIMIT_EXCEEDED</c>) when the error carries one, otherwise the Gemini <c>status</c> (<c>RESOURCE_EXHAUSTED</c>,
/// <c>INVALID_ARGUMENT</c>) — one status covers several reasons that need different handling. The message is Gemini's
/// <c>error.message</c> without the «Details: …» lines the SDK appends; the SDK keeps no response body, so
/// <c>ResponseBody</c> is null.
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

        // Any other HTTP error: the status and Gemini's sentence, not the SDK's exception type.
        if (exception is ApiException { StatusCode: > 0 } api)
        {
            return new ProviderHttpException(SentenceOf(api), (System.Net.HttpStatusCode)api.StatusCode, api)
            {
                RetryAfter = RetryDelayOf(api),
                ErrorCode = ErrorCodeOf(api),
            };
        }

        return null;
    }

    // The SDK writes error.message, then «Details: <json>» lines for the error's details; the sentence is the part before.
    private static string SentenceOf(ApiException exception)
    {
        var message = exception.Message;
        var details = message.IndexOf("\nDetails:", StringComparison.Ordinal);
        return details > 0 ? message[..details].TrimEnd() : message;
    }

    // Google's error model puts the machine-readable reason in a google.rpc.ErrorInfo detail; the status is the coarse
    // class above it. The SDK keeps no structured details: it writes each detail as one JSON line after «Details:» (the
    // stream path also writes the whole error JSON before that), so the reason is read from those lines.
    private static string? ErrorCodeOf(ApiException exception) => ErrorInfoReasonOf(exception.Message) ?? exception.Status;

    private static string? ErrorInfoReasonOf(string message)
    {
        const string marker = "\nDetails:";
        var details = message.IndexOf(marker, StringComparison.Ordinal);
        if (details < 0)
            return null;

        foreach (var line in message[(details + marker.Length)..].Split('\n'))
        {
            var json = line.Trim();
            if (!json.StartsWith('{'))
                continue;
            try
            {
                if (JsonNode.Parse(json) is JsonObject detail
                    && detail["@type"] is JsonValue type && type.TryGetValue<string>(out var typeName)
                    && typeName.EndsWith("google.rpc.ErrorInfo", StringComparison.Ordinal)
                    && detail["reason"] is JsonValue reason && reason.TryGetValue<string>(out var value)
                    && !string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
            catch (JsonException)
            {
                // A line that is not one detail's JSON (a message that itself contains «Details:»): skip it.
            }
        }

        return null;
    }

    // The SDK doesn't expose a dedicated RetryInfo/retryDelay field, so it is read from the details the SDK writes into
    // the message; null when it wrote none.
    private static TimeSpan? RetryDelayOf(ApiException exception)
        => RetryDelayPattern().Match(exception.Message) is { Success: true } match
            && double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds)
                ? TimeSpan.FromSeconds(seconds)
                : null;

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
        Exception exception, bool keyUsesRetiredFormat, out ProviderHttpException? result)
    {
        result = null;
        if (!keyUsesRetiredFormat || exception is not ClientError { StatusCode: 401 or 403 } rejected)
            return false;

        result = new ProviderHttpException(
            $"{SentenceOf(rejected)} The configured API key is in the retired key format, which Gemini stopped " +
            "accepting in September 2026; re-issue the key in Google AI Studio. The library does not check " +
            "the key format, so this is a hint rather than a verdict.",
            (System.Net.HttpStatusCode)rejected.StatusCode,
            rejected)
        {
            ErrorCode = ErrorCodeOf(rejected),
        };
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

        result = new ContextOverflowException(SentenceOf(clientError), clientError)
        {
            ContextWindow = contextWindow,
            RequestTokens = requestTokens,
            ErrorCode = ErrorCodeOf(clientError),
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
            ClientError { StatusCode: 402 } paymentRequired => new BillingException(SentenceOf(paymentRequired), paymentRequired)
            {
                StatusCode = System.Net.HttpStatusCode.PaymentRequired,
                ErrorCode = ErrorCodeOf(paymentRequired),
            },
            ClientError { StatusCode: 429 } quota when quota.Message.Contains("insufficient_quota", StringComparison.OrdinalIgnoreCase)
                => new BillingException(SentenceOf(quota), quota)
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

        result = new RateLimitException(SentenceOf(rateLimited), rateLimited) { RetryAfter = RetryDelayOf(rateLimited), ErrorCode = ErrorCodeOf(rateLimited) };
        return true;
    }
}
