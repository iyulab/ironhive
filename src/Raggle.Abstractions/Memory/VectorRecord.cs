namespace Raggle.Abstractions.Memory;

/// <summary>
/// Represents a vector record.
/// </summary>
public class VectorRecord
{
    /// <summary>
    /// Gets or sets the document ID.
    /// </summary>
    public required string DocumentId { get; set; }

    /// <summary>
    /// Gets or sets the embedding model.
    /// </summary>
    public string? EmbeddingModel { get; set; }

    /// <summary>
    /// Gets or sets the embedding.
    /// </summary>
    public float[] Embedding { get; set; } = [];

    /// <summary>
    /// Gets or sets the tags.
    /// </summary>
    public string[] Tags { get; set; } = [];

    /// <summary>
    /// Gets or sets the metadata.
    /// </summary>
    public object? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the creation date and time.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update date and time.
    /// </summary>
    public DateTime? LastUpdatedAt { get; set; }
}
