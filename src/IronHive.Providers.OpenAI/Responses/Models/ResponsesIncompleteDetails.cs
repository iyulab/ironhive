using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Responses.Models;

internal class ResponsesIncompleteDetails
{
    [JsonPropertyName("reason")]
    public required string Reason { get; set; }
}