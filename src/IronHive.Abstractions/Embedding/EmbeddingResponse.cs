namespace IronHive.Abstractions.Embedding;

/// <summary>
/// The result of one batch embedding call: the vectors, plus what the provider reported about the call.
/// </summary>
public class EmbeddingResponse
{
    /// <summary>One result per input, ordered by <see cref="EmbeddingResult.Index"/>.</summary>
    public required IReadOnlyList<EmbeddingResult> Results { get; init; }

    /// <summary>
    /// Input tokens the provider reports it processed for the call (summed over the requests it was split into), or
    /// null when the provider does not report them — never an estimate. A self-hosted model's own count can differ from
    /// what a local tokenizer (<see cref="IEmbeddingGenerator.CountTokensBatchAsync"/>) computes.
    /// </summary>
    public int? InputTokens { get; init; }

    /// <summary>
    /// The model the provider says answered, which can differ from the requested id (an alias, a pinned revision, a
    /// served file), or null when the provider does not say.
    /// </summary>
    public string? Model { get; init; }
}
