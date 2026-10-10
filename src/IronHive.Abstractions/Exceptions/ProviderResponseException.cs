namespace IronHive.Abstractions.Exceptions;

/// <summary>
/// The provider accepted the request (HTTP 200) and then failed the response: it sent an error inside a stream that had
/// already started (OpenAI Responses <c>error</c> / <c>response.failed</c>, a Chat Completions <c>data: {"error": …}</c>
/// line, an Anthropic <c>event: error</c> such as <c>overloaded_error</c>), the stream ended without the vendor's
/// completion signal (the connection dropped, or the vendor SDK swallowed the error), or a buffered response came back
/// with a failed status and an error object. There is no HTTP error status, so this is not an
/// <see cref="HttpRequestException"/> (that is <see cref="ProviderHttpException"/>); any text received before the failure
/// is incomplete.
/// </summary>
/// <remarks>
/// An error that has a more specific type keeps it here as well: an exhausted balance is <see cref="BillingException"/>,
/// a rate limit <see cref="RateLimitException"/>, an oversized request <see cref="ContextOverflowException"/> (each with a
/// null status). This type is what remains.
/// </remarks>
public class ProviderResponseException : HiveException
{
    public ProviderResponseException(string message, Exception? inner = null)
        : base(message, inner)
    { }

    /// <summary>
    /// The vendor's error code or type as it sent it (<c>server_error</c>, <c>overloaded_error</c>, <c>api_error</c>), or
    /// null when the response ended without an error of its own.
    /// </summary>
    public string? ErrorCode { get; init; }
}
