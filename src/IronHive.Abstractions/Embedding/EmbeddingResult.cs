namespace IronHive.Abstractions.Embedding;

public class EmbeddingResult
{
    /// <summary>
    /// Gets or sets the index of the embedding.
    /// </summary>
    public int? Index { get; set; }

    /// <summary>
    /// Gets or sets the embedding values.
    /// </summary>
    public IEnumerable<float>? Embedding { get; set; }
}
