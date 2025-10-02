using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

internal class ResponsesIncompleteDetails
{
    [JsonPropertyName("reason")]
    public required string Reason { get; set; }
}