namespace Raggle.Abstractions.Memory;

public class ScoredVectorPoint
{
    /// <summary>
    /// Gets or sets the vector ID.
    /// </summary>
    public required Guid VectorId { get; set; }

    /// <summary>
    /// Gest or sets vector similarity score
    /// </summary>
    public required float Score { get; set; }

    /// <summary>
    /// Gets or sets DocumentId
    /// </summary>
    public required string DocumentId { get; set; }

    /// <summary>
    /// Gets or sets Document chunk index
    /// </summary>
    public required int ChunkIndex { get; set; }

    /// <summary>
    /// Gets or sets the QAIndex
    /// </summary>
    public int? QAPairIndex { get; set; }
}
