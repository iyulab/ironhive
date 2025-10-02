using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Embedding;

public class EmbeddingTokenUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}
