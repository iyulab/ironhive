using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using IronHive.Abstractions.Exceptions;

namespace IronHive.Providers.OpenAI.Compatible.ChatCompletion;

/// <summary>
/// Detects context-window overflow in OpenAI-compatible error responses and normalizes
/// them to <see cref="ContextWindowExceededException"/>. Known formats:
/// llama.cpp / GPUStack — type <c>exceed_context_size_error</c>, message
/// "request (42259 tokens) exceeds the available context size (32768 tokens), ...",
/// body may carry <c>n_prompt_tokens</c>/<c>n_ctx</c>;
/// vLLM / OpenAI-compatible — code <c>context_length_exceeded</c>, message
/// "This model's maximum context length is X tokens. However, you requested Y tokens ...".
/// </summary>
internal static partial class ContextOverflowDetector
{
    [GeneratedRegex(@"\((\d+) tokens?\) exceeds the available context size \((\d+) tokens?\)")]
    private static partial Regex LlamaCppPattern();

    [GeneratedRegex(@"maximum context length is (\d+) tokens?", RegexOptions.IgnoreCase)]
    private static partial Regex MaxContextPattern();

    [GeneratedRegex(@"(?:requested|resulted in) (\d+) tokens?", RegexOptions.IgnoreCase)]
    private static partial Regex RequestedTokensPattern();

    private static readonly string[] OverflowMarkers =
    [
        "exceed_context_size_error",
        "context_length_exceeded",
        "exceeds the available context size",
        "maximum context length",
    ];

    /// <summary>
    /// Returns a normalized exception when <paramref name="message"/> (optionally with the raw
    /// error <paramref name="body"/>) indicates a context-window overflow; otherwise null.
    /// </summary>
    public static ContextWindowExceededException? Detect(string message, JsonNode? body = null, Exception? inner = null)
    {
        var errorType = FindString(body, "type");
        var errorCode = FindString(body, "code");

        var isOverflow =
            IsOverflowMarker(errorType) ||
            IsOverflowMarker(errorCode) ||
            OverflowMarkers.Any(m => message.Contains(m, StringComparison.OrdinalIgnoreCase));
        if (!isOverflow)
            return null;

        int? promptTokens = FindInt(body, "n_prompt_tokens");
        int? contextWindow = FindInt(body, "n_ctx");

        if (LlamaCppPattern().Match(message) is { Success: true } llama)
        {
            promptTokens ??= ParseInt(llama.Groups[1].Value);
            contextWindow ??= ParseInt(llama.Groups[2].Value);
        }
        contextWindow ??= MaxContextPattern().Match(message) is { Success: true } max
            ? ParseInt(max.Groups[1].Value) : null;
        promptTokens ??= RequestedTokensPattern().Match(message) is { Success: true } req
            ? ParseInt(req.Groups[1].Value) : null;

        return new ContextWindowExceededException(message, inner)
        {
            PromptTokens = promptTokens,
            ContextWindow = contextWindow,
        };
    }

    private static bool IsOverflowMarker(string? value)
        => value is "exceed_context_size_error" or "context_length_exceeded";

    private static int? ParseInt(string value)
        => int.TryParse(value, out var parsed) ? parsed : null;

    private static string? FindString(JsonNode? node, string key)
    {
        if (node is JsonObject obj)
        {
            foreach (var kvp in obj)
            {
                if (kvp.Key.Equals(key, StringComparison.OrdinalIgnoreCase) && kvp.Value is JsonValue value
                    && value.TryGetValue<string>(out var text))
                    return text;
                if (FindString(kvp.Value, key) is { } found)
                    return found;
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                if (FindString(item, key) is { } found)
                    return found;
            }
        }
        return null;
    }

    private static int? FindInt(JsonNode? node, string key)
    {
        if (node is JsonObject obj)
        {
            foreach (var kvp in obj)
            {
                if (kvp.Key.Equals(key, StringComparison.OrdinalIgnoreCase) && kvp.Value is JsonValue value
                    && value.TryGetValue<int>(out var number))
                    return number;
                if (FindInt(kvp.Value, key) is { } found)
                    return found;
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                if (FindInt(item, key) is { } found)
                    return found;
            }
        }
        return null;
    }
}
