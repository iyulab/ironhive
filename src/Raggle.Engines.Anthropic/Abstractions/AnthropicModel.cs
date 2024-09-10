namespace Raggle.Engines.Anthropic.Abstractions;

/// <summary>
/// Represents a Anthropic model.
/// </summary>
public class AnthropicModel
{
    /// <summary>
    /// the model ID.
    /// </summary>
    public required string ModelId { get; set; }
}
