namespace IronHive.Abstractions.Exceptions;

/// <summary>
/// The request input exceeds the model's context window. Providers normalize their
/// vendor-specific overflow errors (e.g. llama.cpp <c>exceed_context_size_error</c>,
/// OpenAI <c>context_length_exceeded</c>, Anthropic "prompt is too long") to this type
/// so consumers can detect and recover (compact, truncate, re-route) without string parsing.
/// </summary>
public class ContextWindowExceededException : HiveException
{
    public ContextWindowExceededException(string message, Exception? innerException = null)
        : base(message, innerException)
    { }

    /// <summary>Prompt token count reported by the provider (or locally estimated), when known.</summary>
    public int? PromptTokens { get; init; }

    /// <summary>Model context window size, when known.</summary>
    public int? ContextWindow { get; init; }

    /// <summary>
    /// True when the request was rejected locally before any network call
    /// (preflight budget check); false when the provider rejected it.
    /// </summary>
    public bool IsPreflightRejection { get; init; }
}
