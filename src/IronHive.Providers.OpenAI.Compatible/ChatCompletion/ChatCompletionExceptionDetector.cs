using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using IronHive.Abstractions.Exceptions;
using IronHive.Providers.OpenAI;

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
/// The overflow spellings live in <see cref="OpenAIErrors"/> — shared with the OpenAI provider and public for
/// callers with their own SDK client.
/// </summary>
internal static class ChatCompletionExceptionDetector
{
    /// <summary>
    /// Reads and parses a failed HTTP response's error body, then returns the matching domain
    /// exception — falling back to <see cref="ProviderHttpException"/> carrying the extracted (or,
    /// failing that, a synthesized status-line) message, the response's
    /// <see cref="HttpRequestException.StatusCode"/> and its retry hint when the shape isn't recognized.
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

        if (OpenAIErrors.MatchContextOverflow(
                message, type, code, body.FindInt("n_ctx"), body.FindInt("n_prompt_tokens"), inner: null,
                (int)response.StatusCode) is { } overflow)
            return overflow;

        if (IsRateLimit(message, type, code, (int)response.StatusCode))
        {
            return new RateLimitException(message)
            {
                RetryAfter = FindRetryAfter(response),
            };
        }

        // The status is the one fact a caller needs to act on a refusal (401 key, 404 model, 5xx server), and the
        // retry hint tells a busy server (503 + Retry-After) from a broken one.
        return new ProviderHttpException(message, response.StatusCode)
        {
            RetryAfter = FindRetryAfter(response),
        };
    }

    /// <summary>
    /// Detects a known error shape in a bare message with no HTTP response of its own —
    /// falling back to <see cref="HttpRequestException"/> when the shape isn't recognized.
    /// </summary>
    public static Exception Detect(string message)
    {
        if (OpenAIErrors.TryMapContextOverflow(message) is { } overflow)
            return overflow;

        if (IsRateLimit(message, type: null, code: null, status: null))
            return new RateLimitException(message);

        return new HttpRequestException(message);
    }

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

    private static TimeSpan? FindRetryAfter(HttpResponseMessage response)
    {
        // OpenAI-style servers send a millisecond hint next to (or instead of) the standard header.
        if (response.Headers.TryGetValues("retry-after-ms", out var values)
            && double.TryParse(values.FirstOrDefault(), NumberStyles.Float, CultureInfo.InvariantCulture, out var millis)
            && millis >= 0)
            return TimeSpan.FromMilliseconds(millis);

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
