namespace Raggle.Abstractions.Memory;

/// <summary>
/// Represents a vector record.
/// </summary>
public class VectorPoint
{
    /// <summary>
    /// Gets or sets the vector ID.
    /// </summary>
    public Guid VectorId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the vector values.
    /// </summary>
    public float[] Vectors { get; set; } = [];

    /// <summary>
    /// Gets or sets the document ID.
    /// </summary>
    public string? DocumentId { get; set; }

    /// <summary>
    /// Gets or sets the tags.
    /// </summary>
    public string[]? Tags { get; set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public object? Payload { get; set; }
}
