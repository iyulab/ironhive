using System.ClientModel;
using System.Text.RegularExpressions;
using IronHive.Abstractions.Exceptions;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// Normalizes OpenAI SDK context-window overflow errors (code <c>context_length_exceeded</c>,
/// message "This model's maximum context length is X tokens. However, your messages resulted
/// in Y tokens ...") to <see cref="ContextWindowExceededException"/>.
/// </summary>
internal static partial class ContextWindowErrorMapper
{
    [GeneratedRegex(@"maximum context length is (\d+) tokens?", RegexOptions.IgnoreCase)]
    private static partial Regex MaxContextPattern();

    [GeneratedRegex(@"(?:resulted in|requested) (\d+) tokens?", RegexOptions.IgnoreCase)]
    private static partial Regex PromptTokensPattern();

    /// <summary>Returns the normalized exception when <paramref name="exception"/> is an SDK
    /// context-overflow error; otherwise null (leaving the original exception to propagate).</summary>
    public static Exception? TryMap(Exception exception)
        => exception is ClientResultException ? Detect(exception.Message, exception) : null;

    internal static ContextWindowExceededException? Detect(string message, Exception? inner = null)
    {
        if (!message.Contains("context_length_exceeded", StringComparison.OrdinalIgnoreCase) &&
            !message.Contains("maximum context length", StringComparison.OrdinalIgnoreCase))
            return null;

        return new ContextWindowExceededException(message, inner)
        {
            ContextWindow = MatchInt(MaxContextPattern(), message),
            PromptTokens = MatchInt(PromptTokensPattern(), message),
        };
    }

    private static int? MatchInt(Regex pattern, string message)
        => pattern.Match(message) is { Success: true } match && int.TryParse(match.Groups[1].Value, out var value)
            ? value : null;
}
