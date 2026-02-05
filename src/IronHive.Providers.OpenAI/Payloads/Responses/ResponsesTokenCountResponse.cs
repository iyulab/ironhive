using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

public class ResponsesTokenCountResponse
{
    [JsonPropertyName("object")]
    public string Object { get; } = "response.input_tokens";

    [JsonPropertyName("input_tokens")]
    public int InputTokens { get; set; }
}
