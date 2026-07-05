using System.Text.RegularExpressions;
using Anthropic.Exceptions;
using IronHive.Abstractions.Exceptions;

namespace IronHive.Providers.Anthropic;

/// <summary>
/// Normalizes Anthropic SDK context-window overflow errors (invalid_request_error with
/// message "prompt is too long: X tokens > Y maximum") to
/// <see cref="ContextWindowExceededException"/>.
/// </summary>
internal static partial class ContextWindowErrorMapper
{
    [GeneratedRegex(@"prompt is too long:\s*(\d+) tokens? > (\d+) maximum", RegexOptions.IgnoreCase)]
    private static partial Regex PromptTooLongPattern();

    /// <summary>Returns the normalized exception when <paramref name="exception"/> is an SDK
    /// context-overflow error; otherwise null (leaving the original exception to propagate).</summary>
    public static Exception? TryMap(Exception exception)
        => exception is AnthropicException ? Detect(exception.Message, exception) : null;

    internal static ContextWindowExceededException? Detect(string message, Exception? inner = null)
    {
        if (!message.Contains("prompt is too long", StringComparison.OrdinalIgnoreCase))
            return null;

        int? promptTokens = null;
        int? contextWindow = null;
        if (PromptTooLongPattern().Match(message) is { Success: true } match)
        {
            if (int.TryParse(match.Groups[1].Value, out var prompt))
                promptTokens = prompt;
            if (int.TryParse(match.Groups[2].Value, out var window))
                contextWindow = window;
        }

        return new ContextWindowExceededException(message, inner)
        {
            PromptTokens = promptTokens,
            ContextWindow = contextWindow,
        };
    }
}
