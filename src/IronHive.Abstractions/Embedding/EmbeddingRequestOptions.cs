using System.Text.Json.Nodes;

namespace IronHive.Abstractions.Embedding;

/// <summary>
/// Per-call options of an embedding request.
/// </summary>
public sealed class EmbeddingRequestOptions
{
    /// <summary>
    /// Provider-specific request fields, deep-merged into the JSON body the provider sends (an object merges into an
    /// object; any other value replaces the one there, including a field this library sets) — the embedding counterpart
    /// of <see cref="Messages.MessageGenerationRequest.ExtraBody"/>. For server extensions no typed member models, e.g.
    /// a llama.cpp or vLLM pooling option. Honoured by the OpenAI-compatible provider; a provider that cannot extend its
    /// request body throws <see cref="NotSupportedException"/> when this has entries rather than dropping them.
    /// </summary>
    public JsonObject? ExtraBody { get; set; }
}
