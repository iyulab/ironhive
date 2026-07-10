using System.Text.RegularExpressions;
using Google.GenAI;
using IronHive.Abstractions.Exceptions;

namespace IronHive.Providers.GoogleAI;

/// <summary>
/// Normalizes Gemini API errors to IronHive domain exceptions. Currently covers
/// context-window overflow (HTTP 400, <c>status == "INVALID_ARGUMENT"</c>, message "The input
/// token count (X) exceeds the maximum number of tokens allowed (Y).") ->
/// <see cref="ContextOverflowException"/>.
/// </summary>
internal static partial class GoogleAIExceptionMapper
{
    [GeneratedRegex(@"input token count \(\d+\) exceeds the maximum number of tokens allowed \((\d+)\)", RegexOptions.IgnoreCase)]
    private static partial Regex TokenCountExceededPattern();

    /// <summary>Returns the normalized exception when <paramref name="exception"/> matches a
    /// known error shape; otherwise null (leaving the original exception to propagate).</summary>
    public static Exception? Map(Exception exception)
    {
        if (exception is ClientError { StatusCode: 400, Status: "INVALID_ARGUMENT" } clientError)
        {
            var message = clientError.Message;
            if (message.Contains("exceeds the maximum number of tokens allowed", StringComparison.OrdinalIgnoreCase))
            {
                int? contextWindow = null;
                if (TokenCountExceededPattern().Match(message) is { Success: true } match
                    && int.TryParse(match.Groups[1].Value, out var window))
                {
                    contextWindow = window;
                }

                return new ContextOverflowException(message, clientError)
                {
                    ContextWindow = contextWindow,
                };
            }
        }

        return null;
    }
}
