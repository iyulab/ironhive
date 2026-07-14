using System.Globalization;
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
    /// <summary>Returns the normalized exception when <paramref name="exception"/> matches a
    /// known error shape; otherwise null (leaving the original exception to propagate).</summary>
    public static Exception? Map(Exception exception)
    {
        if (IsContextOverflow(exception, out var overflow))
            return overflow;

        if (IsRateLimit(exception, out var rateLimit))
            return rateLimit;

        return null;
    }

    [GeneratedRegex(@"input token count \(\d+\) exceeds the maximum number of tokens allowed \((\d+)\)", RegexOptions.IgnoreCase)]
    private static partial Regex TokenCountExceededPattern();

    private static bool IsContextOverflow(Exception exception, out ContextOverflowException? result)
    {
        result = null;
        if (exception is not ClientError { StatusCode: 400, Status: "INVALID_ARGUMENT" } clientError)
            return false;

        var message = clientError.Message;
        if (!message.Contains("exceeds the maximum number of tokens allowed", StringComparison.OrdinalIgnoreCase))
            return false;

        int? contextWindow = null;
        if (TokenCountExceededPattern().Match(message) is { Success: true } match
            && int.TryParse(match.Groups[1].Value, out var window))
        {
            contextWindow = window;
        }

        result = new ContextOverflowException(message, clientError) { ContextWindow = contextWindow };
        return true;
    }

    [GeneratedRegex(@"""retryDelay""\s*:\s*""(\d+(?:\.\d+)?)s""", RegexOptions.IgnoreCase)]
    private static partial Regex RetryDelayPattern();

    private static bool IsRateLimit(Exception exception, out RateLimitException? result)
    {
        result = null;
        if (exception is not ClientError { StatusCode: 429, Status: "RESOURCE_EXHAUSTED" } rateLimited)
            return false;

        // The SDK doesn't expose a dedicated RetryInfo/retryDelay field, so this falls back
        // to scraping it out of the error message on the off chance the SDK preserved the
        // raw error body there; if not, RetryAfter is left null.
        TimeSpan? retryAfter = null;
        if (RetryDelayPattern().Match(rateLimited.Message) is { Success: true } match
            && double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
        {
            retryAfter = TimeSpan.FromSeconds(seconds);
        }

        result = new RateLimitException(rateLimited.Message, rateLimited) { RetryAfter = retryAfter };
        return true;
    }
}
