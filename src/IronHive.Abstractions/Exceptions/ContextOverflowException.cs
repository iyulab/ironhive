namespace IronHive.Abstractions.Exceptions;

/// <summary>
/// The request input exceeds the model's context window. Providers normalize their
/// vendor-specific overflow errors (e.g. llama.cpp <c>exceed_context_size_error</c>,
/// OpenAI <c>context_length_exceeded</c>, Anthropic "prompt is too long", Gemini
/// "exceeds the maximum number of tokens allowed") to this type so consumers can
/// detect and recover (compact, truncate, re-route) without string parsing.
/// </summary>
public class ContextOverflowException : HiveException
{
    public ContextOverflowException(string message, Exception? inner = null)
        : base(message, inner)
    { }

    /// <summary>Model context window size, when known.</summary>
    public int? ContextWindow { get; set; }
}
