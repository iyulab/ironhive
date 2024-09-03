using System.Text.Json.Serialization;

namespace Raggle.Engines.OpenAI.ChatCompletion.ChatRequestObject;

public class TokenUsage
{
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}
