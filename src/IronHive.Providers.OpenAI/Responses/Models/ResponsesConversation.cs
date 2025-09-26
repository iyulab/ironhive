using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Responses.Models;

public class ResponsesConversation
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
}
