using System.ClientModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using IronHive.Abstractions.Exceptions;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// Maps errors from OpenAI and OpenAI-compatible servers to IronHive's domain exceptions — the same
/// recognition IronHive's own OpenAI and OpenAI-compatible providers apply, for a caller that talks to the
/// server through its own OpenAI SDK client (or <c>Microsoft.Extensions.AI.OpenAI</c>) instead of an
/// IronHive provider.
/// <para>
/// Recognized context-window overflow shapes: OpenAI / vLLM <c>context_length_exceeded</c>
/// ("This model's maximum context length is X tokens. However, you requested Y tokens …"), and
/// llama.cpp / GPUStack <c>exceed_context_size_error</c> ("request (Y tokens) exceeds the available
/// context size (X tokens)", body fields <c>n_ctx</c> / <c>n_prompt_tokens</c>).
/// </para>
/// </summary>
/// <example>
/// <code>
/// try { await chatClient.GetResponseAsync(messages); }
/// catch (Exception ex) when (OpenAIErrors.TryMapContextOverflow(ex) is { } overflow)
/// {
///     // overflow.ContextWindow / overflow.RequestTokens — compact and retry
/// }
/// </code>
/// </example>
public static partial class OpenAIErrors
{
    /// <summary>
    /// Returns a <see cref="ContextOverflowException"/> when <paramref name="exception"/> — or an exception
    /// it wraps — is an OpenAI SDK <see cref="ClientResultException"/> reporting that the request exceeded the
    /// model's context window; otherwise null. The server's error body is read when the SDK kept it, so
    /// <see cref="ContextOverflowException.ContextWindow"/> and <see cref="ContextOverflowException.RequestTokens"/>
    /// are filled whenever the server states them. The original exception is the result's inner exception.
    /// </summary>
    public static ContextOverflowException? TryMapContextOverflow(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        if (FindClientResultException(exception) is not { } clientEx)
            return null;

        var body = ReadBody(clientEx);
        return MatchContextOverflow(
            clientEx.Message,
            body.FindString("type"),
            body.FindString("code"),
            body.FindInt("n_ctx"),
            body.FindInt("n_prompt_tokens"),
            clientEx,
            clientEx.Status);
    }

    /// <summary>
    /// Returns a <see cref="ContextOverflowException"/> when <paramref name="message"/> is a context-window
    /// overflow message with no exception around it — for example an <c>error</c> event inside a streamed
    /// response; otherwise null.
    /// </summary>
    public static ContextOverflowException? TryMapContextOverflow(string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return MatchContextOverflow(message, type: null, code: null, contextWindow: null, requestTokens: null, inner: null, status: null);
    }

    /// <summary>
    /// Returns a <see cref="BillingException"/> when <paramref name="exception"/> — or an exception it wraps — is an
    /// OpenAI SDK <see cref="ClientResultException"/> refusing the request because the account cannot pay: HTTP 402,
    /// or error code/type <c>insufficient_quota</c> (OpenAI sends an exhausted balance or quota as HTTP 429);
    /// otherwise null.
    /// </summary>
    public static BillingException? TryMapBilling(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        if (FindClientResultException(exception) is not { } clientEx)
            return null;

        var body = ReadBody(clientEx);
        return IsBilling(clientEx.Message, body.FindString("type"), body.FindString("code"), clientEx.Status)
            ? new BillingException(clientEx.Message, clientEx)
            : null;
    }

    /// <summary>
    /// Returns a <see cref="RateLimitException"/> when <paramref name="exception"/> — or an exception it wraps —
    /// is an OpenAI SDK <see cref="ClientResultException"/> with HTTP status 429, with
    /// <see cref="RateLimitException.RetryAfter"/> from the <c>retry-after</c> header when present; otherwise null.
    /// A 429 that reports an exhausted balance or quota is a billing refusal, not a rate limit — null here,
    /// <see cref="TryMapBilling"/> maps it.
    /// </summary>
    public static RateLimitException? TryMapRateLimit(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        if (FindClientResultException(exception) is not { Status: 429 } clientEx)
            return null;
        if (TryMapBilling(clientEx) is not null)
            return null;

        // OpenAI's other rate-limit headers (x-ratelimit-remaining-requests etc.) are only
        // meaningful relative to the caller's configured limit, so they aren't surfaced here.
        TimeSpan? retryAfter = null;
        if (clientEx.GetRawResponse()?.Headers.TryGetValue("retry-after", out var value) == true
            && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
        {
            retryAfter = TimeSpan.FromSeconds(seconds);
        }

        return new RateLimitException(clientEx.Message, clientEx) { RetryAfter = retryAfter };
    }

    private static readonly string[] BillingCodes =
    [
        "insufficient_quota",
    ];

    /// <summary>
    /// The one place the billing-refusal spellings live — the SDK path above and the OpenAI-compatible provider's own
    /// HTTP path both come here. HTTP 402 Payment Required is a billing refusal from any server (DeepSeek, OpenRouter,
    /// a metering gateway); OpenAI's <c>insufficient_quota</c> arrives as 429, so the code is checked regardless of
    /// status, and in the message for an error line inside a stream (<paramref name="status"/> null).
    /// </summary>
    internal static bool IsBilling(string message, string? type, string? code, int? status)
        => status == 402
            || BillingCodes.Contains(type, StringComparer.OrdinalIgnoreCase)
            || BillingCodes.Contains(code, StringComparer.OrdinalIgnoreCase)
            || BillingCodes.Any(marker => message.Contains(marker, StringComparison.OrdinalIgnoreCase));

    private static readonly string[] ContextOverflowCodes =
    [
        "exceed_context_size_error",
        "context_length_exceeded",
    ];

    private static readonly string[] ContextOverflowMarkers =
    [
        .. ContextOverflowCodes,
        "exceeds the available context size",
        "maximum context length",
    ];

    /// <summary>
    /// The one place the overflow spellings live — the SDK path above and the OpenAI-compatible provider's own
    /// HTTP path both come here. Explicit numbers (from the error body) win over numbers read from the message.
    /// An overflow is the caller's request being too large, so a server error (<paramref name="status"/> 5xx) is never
    /// one, whatever its text says — llama.cpp answers a failed context shift with <c>500 "… exceeds the available
    /// context size"</c>, and another try can clear that. <paramref name="status"/> is <c>null</c> when there is none
    /// (an error line inside a stream).
    /// </summary>
    internal static ContextOverflowException? MatchContextOverflow(
        string message, string? type, string? code, int? contextWindow, int? requestTokens, Exception? inner, int? status)
    {
        if (status is >= 500)
            return null;

        var matches = ContextOverflowCodes.Contains(type, StringComparer.OrdinalIgnoreCase)
            || ContextOverflowCodes.Contains(code, StringComparer.OrdinalIgnoreCase)
            || ContextOverflowMarkers.Any(marker => message.Contains(marker, StringComparison.OrdinalIgnoreCase));
        if (!matches)
            return null;

        if (LlamaCppPattern().Match(message) is { Success: true } llama)
        {
            requestTokens ??= ParseInt(llama.Groups[1].Value);
            contextWindow ??= ParseInt(llama.Groups[2].Value);
        }
        if (MaxContextPattern().Match(message) is { Success: true } max)
            contextWindow ??= ParseInt(max.Groups[1].Value);
        if (RequestedPattern().Match(message) is { Success: true } requested)
            requestTokens ??= ParseInt(requested.Groups[1].Value);
        if (ResultedInPattern().Match(message) is { Success: true } resulted)
            requestTokens ??= ParseInt(resulted.Groups[1].Value);

        return new ContextOverflowException(message, inner)
        {
            ContextWindow = contextWindow,
            RequestTokens = requestTokens,
        };
    }

    [GeneratedRegex(@"\((\d+) tokens?\) exceeds the available context size \((\d+) tokens?\)", RegexOptions.IgnoreCase)]
    private static partial Regex LlamaCppPattern();

    [GeneratedRegex(@"maximum context length is (\d+) tokens?", RegexOptions.IgnoreCase)]
    private static partial Regex MaxContextPattern();

    [GeneratedRegex(@"you requested (\d+) tokens?", RegexOptions.IgnoreCase)]
    private static partial Regex RequestedPattern();

    // OpenAI Chat Completions: "However, your messages resulted in 9000 tokens."
    [GeneratedRegex(@"resulted in (\d+) tokens?", RegexOptions.IgnoreCase)]
    private static partial Regex ResultedInPattern();

    private static int? ParseInt(string value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;

    private static ClientResultException? FindClientResultException(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is ClientResultException clientEx)
                return clientEx;
            if (current is AggregateException { InnerExceptions.Count: 1 } aggregate)
                return FindClientResultException(aggregate.InnerExceptions[0]);
        }
        return null;
    }

    private static JsonNode? ReadBody(ClientResultException exception)
    {
        try
        {
            var content = exception.GetRawResponse()?.Content;
            return content is null || content.ToMemory().IsEmpty ? null : JsonNode.Parse(content);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            // The SDK did not buffer the body, or the server did not answer JSON — the message still carries
            // the spelling, so recognition falls back to it.
            return null;
        }
    }
}
