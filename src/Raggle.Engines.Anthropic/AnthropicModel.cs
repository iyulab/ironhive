namespace Raggle.Engines.Anthropic;

public enum AnthropicModelType
{
    /// <summary>
    /// Anthropic's Chat Complete model.
    /// </summary>
    Claude,
}

/// <summary>
/// Represents a Anthropic model.
/// </summary>
public class AnthropicModel
{
    /// <summary>
    /// the model type.
    /// </summary>
    public AnthropicModelType Type { get; } = AnthropicModelType.Claude;

    /// <summary>
    /// the model ID.
    /// </summary>
    public string ModelId { get; set; } = string.Empty;
}
