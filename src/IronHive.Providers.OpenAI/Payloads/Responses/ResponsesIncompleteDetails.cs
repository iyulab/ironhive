using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

public class ResponsesIncompleteDetails
{
    [JsonPropertyName("reason")]
    public required string Reason { get; set; }
}