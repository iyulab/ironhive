namespace Raggle.Abstractions.ChatCompletion;

/// <summary>
/// Represents the result of message response
/// </summary>
/// <typeparam name="T"></typeparam>
public class ChatCompletionResult<T>
{
    public EndReason? EndReason { get; set; }

    public T? Data { get; set; }

    public TokenUsage? TokenUsage { get; set; }
}

/// <summary>
/// Represents the stop reason for the chat completion of an LLM operation.
/// </summary>
public enum EndReason
{
    /// <summary>
    /// AI turn was completed.
    /// </summary>
    EndTurn,

    /// <summary>
    /// the maximum number of tokens was reached.
    /// </summary>
    MaxTokens,

    /// <summary>
    /// a stop sequence was encountered.
    /// </summary>
    StopSequence,

    /// <summary>
    /// filtered information
    /// </summary>
    ContentFilter,

    /// <summary>
    /// Tool Calling 
    /// </summary>
    ToolCall,

    /// <summary>
    /// Tool Failure
    /// </summary>
    ToolFailed,
}

/// <summary>
/// Represents the usage of tokens in an AI context.
/// </summary>
public class TokenUsage
{
    /// <summary>
    /// Gets or sets the total number of tokens used.
    /// </summary>
    public int? TotalTokens { get; set; }

    /// <summary>
    /// Gets or sets the number of input tokens used.
    /// </summary>
    public int? InputTokens { get; set; }

    /// <summary>
    /// Gets or sets the number of output tokens used.
    /// </summary>
    public int? OutputTokens { get; set; }
}
