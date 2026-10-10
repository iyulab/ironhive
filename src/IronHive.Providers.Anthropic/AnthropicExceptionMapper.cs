using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Anthropic.Exceptions;
using Anthropic.Models;
using IronHive.Abstractions.Exceptions;

namespace IronHive.Providers.Anthropic;

/// <summary>
/// Normalizes Anthropic SDK errors — an HTTP error, or an <c>event: error</c> inside a stream that had already started —
/// to IronHive domain exceptions. Currently covers
/// context-window overflow (invalid_request_error with message "prompt is too long:
/// X tokens > Y maximum") -> <see cref="ContextOverflowException"/>, a billing refusal -> <see cref="BillingException"/>,
/// a rate limit -> <see cref="RateLimitException"/>, any other error inside a stream (<c>overloaded_error</c>,
/// <c>api_error</c>) -> <see cref="ProviderResponseException"/>, and a request that
/// was cut short by the SDK's own network timeout -> <see cref="TimeoutException"/>.
/// </summary>
internal static partial class AnthropicExceptionMapper
{
    /// <summary>
    /// Returns the normalized exception when <paramref name="exception"/> matches a known
    /// error shape; otherwise null (leaving the original exception to propagate). Gates
    /// structurally on the SDK's <c>ErrorType</c> (<see cref="AnthropicApiException"/> over HTTP,
    /// <see cref="AnthropicSseException"/> inside a stream) — not a base-type catch-all — and reads the
    /// message from the error body (<c>ResponseBody</c>, or the event data the SSE exception carries),
    /// not <c>.Message</c>, which the SDK prefixes with <c>"Status Code: {code}"</c>.
    /// </summary>
    public static Exception? Map(Exception exception, CancellationToken cancellationToken = default)
    {
        if (IsTimeout(exception, cancellationToken, out var timeout))
            return timeout;

        if (IsContextOverflow(exception, out var overflow))
            return overflow;

        if (IsBilling(exception, out var billing))
            return billing;

        if (IsRateLimit(exception, out var rateLimit))
            return rateLimit;

        // An error event after the stream started that none of the above recognised (overloaded_error, api_error, …):
        // it ended the response all the same.
        if (exception is AnthropicSseException sse)
            return StreamError(sse);

        return null;
    }

    /// <summary>
    /// The error body and type of an Anthropic error, wherever it arrived: an HTTP error (<see cref="AnthropicApiException"/>,
    /// body in <c>ResponseBody</c>) or an <c>event: error</c> inside a stream that had already started
    /// (<see cref="AnthropicSseException"/>, body inside its message and no status). Null for anything else.
    /// </summary>
    private static (ErrorType? Type, string Body, System.Net.HttpStatusCode? Status)? ReadError(Exception exception)
        => exception switch
        {
            AnthropicApiException apiEx => (apiEx.ErrorType, apiEx.ResponseBody ?? apiEx.Message, apiEx.StatusCode),
            AnthropicSseException sse => (sse.ErrorType, SseBody(sse), null),
            _ => null,
        };

    private const string SsePrefix = "SSE error returned from server: '";

    // The SDK writes the event's data into the message as «SSE error returned from server: '<json>'».
    private static string SseBody(AnthropicSseException sse)
    {
        var message = sse.Message;
        return message.StartsWith(SsePrefix, StringComparison.Ordinal) && message.EndsWith('\'')
            ? message[SsePrefix.Length..^1]
            : message;
    }

    private static ProviderResponseException StreamError(AnthropicSseException sse)
    {
        var body = SseBody(sse);
        var message = ErrorField(body, "message");
        return new ProviderResponseException(message is { Length: > 0 } ? message : body, sse) { ErrorCode = ErrorField(body, "type") };
    }

    // The vendor's error.type as written (billing_error, rate_limit_error, or a gateway's own code), from the error body.
    private static string? ErrorTypeText(string body) => ErrorField(body, "type");

    private static string? ErrorField(string body, string name)
    {
        try
        {
            return JsonNode.Parse(body)?["error"]?[name] is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// The account cannot pay: <c>billing_error</c>, HTTP 402 whatever the body says (a gateway in front of the API may not
    /// send Anthropic's error shape), or the «credit balance is too low» refusal the API has sent as an
    /// <c>invalid_request_error</c> (HTTP 400) — the same condition under its older shape.
    /// </summary>
    private static bool IsBilling(Exception exception, out BillingException? result)
    {
        result = null;
        if (ReadError(exception) is not var (type, message, status))
            return false;

        var isBilling = type == ErrorType.BillingError
            || status == System.Net.HttpStatusCode.PaymentRequired
            || (type == ErrorType.InvalidRequestError
                && message.Contains("credit balance is too low", StringComparison.OrdinalIgnoreCase));
        if (!isBilling)
            return false;

        result = new BillingException(message, exception) { StatusCode = status, ErrorCode = ErrorTypeText(message) };
        return true;
    }

    /// <summary>
    /// The SDK's internal network timeout cancels the request via its own, unrelated
    /// <see cref="CancellationTokenSource"/>, which surfaces to the caller as a bare
    /// <see cref="OperationCanceledException"/> — indistinguishable from the caller's own
    /// <paramref name="cancellationToken"/> being canceled unless checked here. Only when
    /// that token was NOT the source is this really a timeout.
    /// </summary>
    private static bool IsTimeout(Exception exception, CancellationToken cancellationToken, out TimeoutException? result)
    {
        result = null;
        if (exception is not OperationCanceledException || cancellationToken.IsCancellationRequested)
            return false;

        result = new TimeoutException("The Anthropic request timed out.", exception);
        return true;
    }

    [GeneratedRegex(@"prompt is too long:\s*(\d+) tokens? > (\d+) maximum", RegexOptions.IgnoreCase)]
    private static partial Regex PromptTooLongPattern();

    private static bool IsContextOverflow(Exception exception, out ContextOverflowException? result)
    {
        result = null;
        if (ReadError(exception) is not (ErrorType.InvalidRequestError, var message, _))
            return false;

        if (!message.Contains("prompt is too long", StringComparison.OrdinalIgnoreCase))
            return false;

        int? contextWindow = null;
        int? requestTokens = null;
        if (PromptTooLongPattern().Match(message) is { Success: true } match)
        {
            if (int.TryParse(match.Groups[1].Value, out var requested))
                requestTokens = requested;
            if (int.TryParse(match.Groups[2].Value, out var window))
                contextWindow = window;
        }

        result = new ContextOverflowException(message, exception)
        {
            ContextWindow = contextWindow,
            RequestTokens = requestTokens,
            ErrorCode = ErrorTypeText(message),
        };
        return true;
    }

    private static bool IsRateLimit(Exception exception, out RateLimitException? result)
    {
        result = null;
        if (ReadError(exception) is not (ErrorType.RateLimitError, var message, _))
            return false;

        // The SDK exposes no response headers, so anthropic-ratelimit-*/retry-after
        // (see https://platform.claude.com/docs/en/api/rate-limits) aren't reachable here;
        // RetryAfter is left null.
        result = new RateLimitException(message, exception) { ErrorCode = ErrorTypeText(message) };
        return true;
    }
}
