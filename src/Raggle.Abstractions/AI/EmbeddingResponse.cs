namespace Raggle.Abstractions.AI;

/// <summary>
/// Represents a response containing an embedding.
/// </summary>
public class EmbeddingResponse
{
    /// <summary>
    /// Gets or sets the index of the embedding.
    /// </summary>
    public int? Index { get; set; }

    /// <summary>
    /// Gets or sets the embedding values.
    /// </summary>
    public float[]? Embedding { get; set; }
}
