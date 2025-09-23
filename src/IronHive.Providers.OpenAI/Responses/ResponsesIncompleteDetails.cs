using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Responses;

internal class ResponsesIncompleteDetails
{
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}