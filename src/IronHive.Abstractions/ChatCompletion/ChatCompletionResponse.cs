namespace IronHive.Abstractions.ChatCompletion;

/// <summary>
/// Represents the result of message response
/// </summary>
/// <typeparam name="T">Type of Data property</typeparam>
public class ChatCompletionResponse<T>
{
    /// <summary>
    /// the response ended reason.
    /// </summary>
    public EndReason? EndReason { get; set; }

    /// <summary>
    /// the usage of tokens in the request.
    /// </summary>
    public TokenUsage? TokenUsage { get; set; }

    /// <summary>
    /// the data of the response.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Created DateTime
    /// </summary>
    public DateTime? Timestamp { get; set; }
}

/// <summary>
/// Represents the stop reason for the chat completion of an LLM operation.
/// </summary>
public enum EndReason
{
    /// <summary>
    /// Assistant turn was completed.
    /// </summary>
    EndTurn,

    /// <summary>
    /// the output maximum number of tokens was reached.
    /// </summary>
    MaxTokens,

    /// <summary>
    /// a stop sequence text was reached.
    /// </summary>
    StopSequence,

    /// <summary>
    /// filtered text content was detected.
    /// </summary>
    ContentFilter,

    /// <summary>
    /// the model calling the tool for execution.
    /// </summary>
    ToolCall
}

/// <summary>
/// Represents the usage of tokens in an LLM context.
/// </summary>
public class TokenUsage
{
    /// <summary>
    /// the number of input tokens used.
    /// </summary>
    public int? InputTokens { get; set; }

    /// <summary>
    /// the number of output tokens used.
    /// </summary>
    public int? OutputTokens { get; set; }

    /// <summary>
    /// the total number of tokens used.
    /// </summary>
    public int? TotalTokens
    {
        get => InputTokens + OutputTokens;
    }
}
