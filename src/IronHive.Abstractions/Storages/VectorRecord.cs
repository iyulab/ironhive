using IronHive.Abstractions.Memory;

namespace IronHive.Abstractions.Storages;

/// <summary>
/// Represents a vector record.
/// </summary>
public class VectorRecord
{
    /// <summary>
    /// the unique identifier for the vector in Guid format.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// the source identifier for the vector.
    /// </summary>
    public required string SourceId { get; set; }

    /// <summary>
    /// the vector values. 
    /// </summary>
    public required IEnumerable<float> Vectors { get; set; }

    /// <summary>
    /// the memory source associated with the vector values.
    /// </summary>
    public required IMemorySource Source { get; set; }

    /// <summary>
    /// the payload information associated with the vector values.
    /// </summary>
    public object? Content { get; set; }

    /// <summary>
    /// the date and time when the record was created or last updated.
    /// </summary>
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}
