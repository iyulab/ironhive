using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

public class ResponsesConversation
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
}
