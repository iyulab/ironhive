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
/// null status). This type is what remains. <see cref="HiveException.ErrorCode"/> carries the vendor's code
/// (<c>server_error</c>, <c>overloaded_error</c>), null when the response ended without an error of its own.
/// </remarks>
public class ProviderResponseException : HiveException
{
    public ProviderResponseException(string message, Exception? inner = null)
        : base(message, inner)
    { }

    /// <summary>
    /// The HTTP status the vendor documents for the same error when it is not inside a stream — Anthropic
    /// <c>overloaded_error</c> 529, <c>api_error</c> 500, <c>invalid_request_error</c> 400; OpenAI <c>server_error</c> 500 — so a
    /// caller that classifies HTTP failures by status (retry the same provider, fall back, give up) treats the mid-stream and
    /// the HTTP form of one error alike. Null when the vendor documents no status for the code, or the response ended
    /// without an error of its own. It is not a status this response had: the response was a 200, and a stream that
    /// already delivered output cannot be retried without repeating it.
    /// </summary>
    public System.Net.HttpStatusCode? EquivalentStatusCode { get; init; }
}
