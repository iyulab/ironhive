using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using IronHive.Abstractions.Exceptions;

namespace IronHive.Providers.OpenAI.Compatible.ChatCompletion;

/// <summary>
/// Detects errors in OpenAI-compatible chat completion responses and normalizes them to
/// IronHive domain exceptions. Currently covers context-window overflow. Known formats:
/// llama.cpp / GPUStack — type <c>exceed_context_size_error</c>, message
/// "request (42259 tokens) exceeds the available context size (32768 tokens), ...",
/// body may carry <c>n_ctx</c>;
/// vLLM / OpenAI-compatible — code <c>context_length_exceeded</c>, message
/// "This model's maximum context length is X tokens. However, you requested Y tokens ...".
/// </summary>
internal static partial class ChatCompletionExceptionDetector
{
    private static readonly string[] ContextOverflowMarkers =
    [
        "exceed_context_size_error",
        "context_length_exceeded",
        "exceeds the available context size",
        "maximum context length",
    ];

    [GeneratedRegex(@"\(\d+ tokens?\) exceeds the available context size \((\d+) tokens?\)")]
    private static partial Regex LlamaCppPattern();

    [GeneratedRegex(@"maximum context length is (\d+) tokens?", RegexOptions.IgnoreCase)]
    private static partial Regex MaxContextPattern();

    /// <summary>
    /// Returns a normalized exception when <paramref name="message"/> (optionally with the raw
    /// error <paramref name="body"/>) matches a known error shape; otherwise null.
    /// </summary>
    public static HiveException? Detect(string message, JsonNode? body = null, Exception? inner = null)
    {
        if (IsContextOverflow(message, body))
        {
            return new ContextOverflowException(message, inner)
            {
                ContextWindow = FindContextWindow(message, body),
            };
        }

        return null;
    }

    private static bool IsContextOverflow(string message, JsonNode? body)
    {
        var errorType = FindString(body, "type");
        var errorCode = FindString(body, "code");

        return IsContextOverflowMarker(errorType)
            || IsContextOverflowMarker(errorCode)
            || ContextOverflowMarkers.Any(marker => message.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsContextOverflowMarker(string? value)
        => value is "exceed_context_size_error" or "context_length_exceeded";

    private static int? FindContextWindow(string message, JsonNode? body)
    {
        if (FindInt(body, "n_ctx") is { } fromBody)
            return fromBody;

        if (LlamaCppPattern().Match(message) is { Success: true } llama)
            return ParseInt(llama.Groups[1].Value);

        if (MaxContextPattern().Match(message) is { Success: true } max)
            return ParseInt(max.Groups[1].Value);

        return null;
    }

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
