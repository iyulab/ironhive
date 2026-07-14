using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using IronHive.Abstractions.Exceptions;

namespace IronHive.Providers.OpenAI.Compatible.ChatCompletion;

/// <summary>
/// Detects errors in OpenAI-compatible chat completion responses and normalizes them to
/// IronHive domain exceptions (context-window overflow, rate limiting). Two entry points,
/// matching the two shapes errors arrive in: <see cref="DetectAsync"/> for a failed HTTP
/// response (reads and parses the error body itself), and <see cref="Detect(string)"/> for a
/// bare mid-stream error line that has no response of its own — see
/// ChatCompletionHttpClient.PostStreamingAsync's "error:" line. Known formats:
/// llama.cpp / GPUStack — type <c>exceed_context_size_error</c>, message
/// "request (42259 tokens) exceeds the available context size (32768 tokens), ...",
/// body may carry <c>n_ctx</c>;
/// vLLM / OpenAI-compatible — code <c>context_length_exceeded</c>, message
/// "This model's maximum context length is X tokens. However, you requested Y tokens ...".
/// </summary>
internal static partial class ChatCompletionExceptionDetector
{
    /// <summary>
    /// Reads and parses a failed HTTP response's error body, then returns the matching domain
    /// exception — falling back to <see cref="HttpRequestException"/> carrying the extracted (or,
    /// failing that, a synthesized status-line) message when the shape isn't recognized.
    /// </summary>
    public static async Task<Exception> DetectAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        JsonNode? body = null;
        try
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            body = JsonNode.Parse(content);
        }
        catch (JsonException)
        { }

        var message = body.FindString("message") is { Length: > 0 } found
            ? found
            : $"Chat completion request failed with status {(int)response.StatusCode} ({response.ReasonPhrase}).";
        var type = body.FindString("type");
        var code = body.FindString("code");

        if (IsContextOverflow(message, type, code))
        {
            return new ContextOverflowException(message)
            {
                ContextWindow = FindContextWindow(message, body),
            };
        }

        if (IsRateLimit(message, type, code, (int)response.StatusCode))
        {
            return new RateLimitException(message)
            {
                RetryAfter = FindRetryAfter(response),
            };
        }

        return new HttpRequestException(message);
    }

    /// <summary>
    /// Detects a known error shape in a bare message with no HTTP response of its own —
    /// falling back to <see cref="HttpRequestException"/> when the shape isn't recognized.
    /// </summary>
    public static Exception Detect(string message)
    {
        if (IsContextOverflow(message, type: null, code: null))
        {
            return new ContextOverflowException(message)
            {
                ContextWindow = FindContextWindow(message, body: null),
            };
        }

        if (IsRateLimit(message, type: null, code: null, status: null))
            return new RateLimitException(message);

        return new HttpRequestException(message);
    }

    private static readonly string[] ContextOverflowMarkers =
    [
        "exceed_context_size_error",
        "context_length_exceeded",
        "exceeds the available context size",
        "maximum context length",
    ];

    private static bool IsContextOverflow(string message, string? type, string? code)
        => type is "exceed_context_size_error" or "context_length_exceeded"
            || code is "exceed_context_size_error" or "context_length_exceeded"
            || ContextOverflowMarkers.Any(marker => message.Contains(marker, StringComparison.OrdinalIgnoreCase));

    private static readonly string[] RateLimitMarkers =
    [
        "rate_limit_exceeded",
        "insufficient_quota",
    ];

    private static bool IsRateLimit(string message, string? type, string? code, int? status)
        // 429 is unambiguous (HTTP "Too Many Requests") regardless of body shape, so it's
        // checked first; the marker fallback covers mid-stream errors reported without a
        // status code.
        => status == 429
            || RateLimitMarkers.Contains(type, StringComparer.OrdinalIgnoreCase)
            || RateLimitMarkers.Contains(code, StringComparer.OrdinalIgnoreCase)
            || RateLimitMarkers.Any(marker => message.Contains(marker, StringComparison.OrdinalIgnoreCase));

    [GeneratedRegex(@"\(\d+ tokens?\) exceeds the available context size \((\d+) tokens?\)")]
    private static partial Regex LlamaCppPattern();

    [GeneratedRegex(@"maximum context length is (\d+) tokens?", RegexOptions.IgnoreCase)]
    private static partial Regex MaxContextPattern();

    private static int? FindContextWindow(string message, JsonNode? body)
    {
        if (body.FindInt("n_ctx") is { } fromBody)
            return fromBody;

        if (LlamaCppPattern().Match(message) is { Success: true } llama)
            return ParseInt(llama.Groups[1].Value);

        if (MaxContextPattern().Match(message) is { Success: true } max)
            return ParseInt(max.Groups[1].Value);

        return null;
    }

    private static int? ParseInt(string value)
        => int.TryParse(value, out var parsed) ? parsed : null;

    private static TimeSpan? FindRetryAfter(HttpResponseMessage response)
    {
        var retryAfter = response.Headers.RetryAfter;
        if (retryAfter is null)
            return null;

        if (retryAfter.Delta is { } delta)
            return delta;

        return retryAfter.Date is { } date && date > DateTimeOffset.UtcNow
            ? date - DateTimeOffset.UtcNow
            : null;
    }
}
