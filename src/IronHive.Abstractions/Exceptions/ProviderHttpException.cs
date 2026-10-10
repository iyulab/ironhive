using System.Net;

namespace IronHive.Abstractions.Exceptions;

/// <summary>
/// A provider answered with an HTTP error that has no more specific IronHive type (rate limits are
/// <see cref="RateLimitException"/>, context-window overflows <see cref="ContextOverflowException"/>, billing
/// refusals <see cref="BillingException"/>). It is an
/// <see cref="HttpRequestException"/> with the response's <see cref="HttpRequestException.StatusCode"/>, so code that
/// handles HTTP failures by status keeps working, and it adds the provider's retry hint: a 503 that sent
/// <c>Retry-After</c> is asking the caller to wait, which a gateway can only honour if the hint survives.
/// </summary>
public class ProviderHttpException : HttpRequestException
{
    public ProviderHttpException(string message, HttpStatusCode statusCode, Exception? inner = null)
        : base(message, inner, statusCode)
    { }

    /// <summary>How long the provider asked the caller to wait before retrying (<c>Retry-After</c>, seconds or an
    /// HTTP date), or null when the response carried no hint.</summary>
    public TimeSpan? RetryAfter { get; init; }

    /// <summary>The error code or type the server put in its error body (<c>error.code</c>, else <c>error.type</c>), or null
    /// when it sent none — the same reading as <see cref="HiveException.ErrorCode"/> on IronHive's typed exceptions.</summary>
    public string? ErrorCode { get; init; }
}
