namespace Raggle.Abstractions.AI;

/// <summary>
/// Represents the stop reason for the chat completion of an LLM operation.
/// </summary>
public enum ChatCompletionEndReason
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
    ContentFilter
}
