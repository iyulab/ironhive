namespace Raggle.Abstractions.AI;

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
