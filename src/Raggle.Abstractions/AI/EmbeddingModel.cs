namespace Raggle.Abstractions.AI;

public class EmbeddingModel
{
    /// <summary>
    /// The Embedding Model Name.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// The Embedding Model Created At.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// The Embedding Model Modified At.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// The Embedding Model Owner.
    /// </summary>
    public string? Owner { get; set; }
}
