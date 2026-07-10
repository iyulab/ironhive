using System.ClientModel;
using System.Text.RegularExpressions;
using IronHive.Abstractions.Exceptions;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// Normalizes OpenAI SDK errors to IronHive domain exceptions. Currently covers Responses API
/// context-window overflow (<c>error.code == "context_length_exceeded"</c>, which the SDK embeds
/// directly into <see cref="ClientResultException"/>'s <c>Message</c>) -> <see cref="ContextOverflowException"/>.
/// <para>
/// Unlike the legacy Chat Completions format ("This model's maximum context length is X
/// tokens..."), the Responses API's overflow message ("Your input exceeds the context window
/// of this model. Please adjust your input and try again.") carries no token counts.
/// <see cref="ContextOverflowException.ContextWindow"/> is therefore expected to be null in
/// practice here — the legacy-format regex is kept only as a best-effort fallback.
/// </para>
/// </summary>
internal static partial class OpenAIExceptionMapper
{
    [GeneratedRegex(@"maximum context length is (\d+) tokens?", RegexOptions.IgnoreCase)]
    private static partial Regex MaxContextPattern();

    /// <summary>Returns the normalized exception when <paramref name="exception"/> matches a
    /// known error shape; otherwise null (leaving the original exception to propagate).</summary>
    public static Exception? Map(Exception exception)
    {
        if (exception is ClientResultException clientEx)
        {
            var message = clientEx.Message;
            if (message.Contains("context_length_exceeded", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("maximum context length", StringComparison.OrdinalIgnoreCase))
            {
                int? contextWindow = null;
                if (MaxContextPattern().Match(message) is { Success: true } match
                    && int.TryParse(match.Groups[1].Value, out var window))
                {
                    contextWindow = window;
                }

                return new ContextOverflowException(message, clientEx)
                {
                    ContextWindow = contextWindow,
                };
            }
        }

        return null;
    }
}
