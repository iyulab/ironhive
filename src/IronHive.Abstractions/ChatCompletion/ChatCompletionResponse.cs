namespace IronHive.Abstractions.ChatCompletion;

/// <summary>
/// Represents the result of message response
/// </summary>
/// <typeparam name="T">Type of Data property</typeparam>
public class ChatCompletionResponse<T>
{
    public EndReason? EndReason { get; set; }

    public TokenUsage? TokenUsage { get; set; }

    public T? Data { get; set; }
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
    ToolCall
}

/// <summary>
/// Represents the usage of tokens in an LLM context.
/// </summary>
public class TokenUsage
{
    /// <summary>
    /// the total number of tokens used.
    /// </summary>
    public int? TotalTokens
    {
        get => InputTokens + OutputTokens;
    }

    /// <summary>
    /// the number of input tokens used.
    /// </summary>
    public int? InputTokens { get; set; }

    /// <summary>
    /// the number of output tokens used.
    /// </summary>
    public int? OutputTokens { get; set; }
}
