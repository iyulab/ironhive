using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Compatible.Reranking;

/// <summary>
/// <c>POST /rerank</c> request body, Cohere-shaped (<see href="https://docs.cohere.com/reference/rerank"/>).
/// </summary>
public class CohereRerankRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("query")]
    public required string Query { get; set; }

    [JsonPropertyName("documents")]
    public required List<string> Documents { get; set; }

    [JsonPropertyName("top_n")]
    public int? TopN { get; set; }
}

/// <summary>
/// <c>POST /rerank</c> response body, Cohere-shaped.
/// </summary>
public class CohereRerankResponse
{
    [JsonPropertyName("results")]
    public required List<CohereRerankResultItem> Results { get; set; }
}

/// <summary>
/// One entry of <see cref="CohereRerankResponse.Results"/>.
/// </summary>
public class CohereRerankResultItem
{
    [JsonPropertyName("index")]
    public required int Index { get; set; }

    [JsonPropertyName("relevance_score")]
    public required float RelevanceScore { get; set; }
}
