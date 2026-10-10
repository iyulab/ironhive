namespace IronHive.Abstractions.Exceptions;

/// <summary>
/// Base type for IronHive domain exceptions. Providers and pipeline components raise
/// subtypes of this exception so consumers can write typed recovery logic instead of
/// parsing provider-specific error strings.
/// </summary>
public class HiveException : Exception
{
    public HiveException()
    { }

    public HiveException(string message)
        : base(message)
    { }

    public HiveException(string message, Exception? inner)
        : base(message, inner)
    { }

    /// <summary>
    /// The provider's own error code or type for this failure, as it sent it (OpenAI <c>error.code</c> such as
    /// <c>insufficient_quota</c> or <c>server_error</c>, Anthropic <c>error.type</c> such as <c>overloaded_error</c>, Gemini
    /// <c>status</c> such as <c>RESOURCE_EXHAUSTED</c>, or whatever code a gateway in front of them puts in the error body),
    /// or null when the failure did not come with one. The type says what to do (wait, top up, shorten); this says which
    /// of the vendor's reasons it was — two billing refusals with different codes may need different handling (an
    /// exhausted balance against a per-key or per-project spending limit).
    /// </summary>
    public string? ErrorCode { get; init; }
}
