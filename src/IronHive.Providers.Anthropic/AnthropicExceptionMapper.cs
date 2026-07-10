using System.Text.RegularExpressions;
using Anthropic.Exceptions;
using Anthropic.Models;
using IronHive.Abstractions.Exceptions;

namespace IronHive.Providers.Anthropic;

/// <summary>
/// Normalizes Anthropic SDK errors to IronHive domain exceptions. Currently covers
/// context-window overflow (invalid_request_error with message "prompt is too long:
/// X tokens > Y maximum") -> <see cref="ContextOverflowException"/>.
/// </summary>
internal static partial class AnthropicExceptionMapper
{
    [GeneratedRegex(@"prompt is too long:\s*\d+ tokens? > (\d+) maximum", RegexOptions.IgnoreCase)]
    private static partial Regex PromptTooLongPattern();

    /// <summary>
    /// Returns the normalized exception when <paramref name="exception"/> matches a known
    /// error shape; otherwise null (leaving the original exception to propagate). Gates
    /// structurally on <see cref="AnthropicApiException"/>'s <c>ErrorType</c> — not a
    /// base-type catch-all — and reads the message from <c>ResponseBody</c> (the raw JSON),
    /// not <c>.Message</c>, which the SDK prefixes with <c>"Status Code: {code}"</c>.
    /// </summary>
    public static Exception? Map(Exception exception)
    {
        if (exception is AnthropicApiException { ErrorType: ErrorType.InvalidRequestError } apiEx)
        {
            var message = apiEx.ResponseBody ?? apiEx.Message;
            if (message.Contains("prompt is too long", StringComparison.OrdinalIgnoreCase))
            {
                int? contextWindow = null;
                if (PromptTooLongPattern().Match(message) is { Success: true } match
                    && int.TryParse(match.Groups[1].Value, out var window))
                {
                    contextWindow = window;
                }

                return new ContextOverflowException(message, apiEx)
                {
                    ContextWindow = contextWindow,
                };
            }
        }

        return null;
    }
}
