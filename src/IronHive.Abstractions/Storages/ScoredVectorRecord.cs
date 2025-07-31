using IronHive.Abstractions.Memory;

namespace IronHive.Abstractions.Storages;

/// <summary>
/// Represents a vector with its score.
/// </summary>
public class ScoredVectorRecord
{
    /// <summary>
    /// the unique identifier of the vector.
    /// </summary>
    public required string VectorId { get; set; }

    /// <summary>
    /// the score of the vector.
    /// </summary>
    public required float Score { get; set; }

    /// <summary>
    /// the memory source associated with the vector.
    /// </summary>
    public required IMemorySource Source { get; set; }

    /// <summary>
    /// the payload information associated with the vector.
    /// </summary>
    public object? Content { get; set; }

    /// <summary>
    /// the date and time when the vector was last updated.
    /// </summary>
    public required DateTime LastUpdatedAt { get; set; }
}
