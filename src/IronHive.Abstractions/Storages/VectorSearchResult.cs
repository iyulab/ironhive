namespace IronHive.Abstractions.Storages;

/// <summary>
/// Represents the result of a vector search.
/// </summary>
public class VectorSearchResult
{
    /// <summary>
    /// the name of the collection where the search was performed.
    /// </summary>
    public required string CollectionName { get; set; }

    /// <summary>
    /// the query used for the search.
    /// </summary>
    public string? SearchQuery { get; set; }

    /// <summary>
    /// the list of vectors with their scores.
    /// </summary>
    public required IEnumerable<ScoredVectorRecord> ScoredVectors { get; set; }
}


