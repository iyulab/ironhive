namespace IronHive.Abstractions.Message;

/// <summary>
/// Represents the stop reason for the chat completion of an LLM operation.
/// </summary>
public enum MessageDoneReason
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
