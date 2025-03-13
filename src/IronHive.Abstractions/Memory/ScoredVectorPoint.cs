namespace IronHive.Abstractions.Memory;

public class ScoredVectorPoint
{
    /// <summary>
    /// Gets or sets the vector ID.
    /// </summary>
    public Guid VectorId { get; set; }

    /// <summary>
    /// Gest or sets vector similarity score
    /// </summary>
    public float Score { get; set; }

    /// <summary>
    /// Gets or sets the document ID.
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tags.
    /// </summary>
    public string[]? Tags { get; set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public object? Payload { get; set; }
}
