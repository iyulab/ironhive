namespace IronHive.Abstractions.Message;

/// <summary>
/// Represents the usage of tokens in an LLM context.
/// </summary>
public class MessageTokenUsage
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
