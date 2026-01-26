using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

internal class ResponsesTokenCountResponse
{
    [JsonPropertyName("object")]
    public string Object { get; } = "response.input_tokens";

    [JsonPropertyName("input_tokens")]
    public int InputTokens { get; set; }
}
