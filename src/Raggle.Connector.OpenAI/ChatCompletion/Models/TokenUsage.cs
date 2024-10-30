using System.Text.Json.Serialization;

namespace Raggle.Connector.OpenAI.ChatCompletion.Models;

internal class TokenUsage
{
    [JsonPropertyName("prompt_tokens")]
    internal int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    internal int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    internal int TotalTokens { get; set; }
}
