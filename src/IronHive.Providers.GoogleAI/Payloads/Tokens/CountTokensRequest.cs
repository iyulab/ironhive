using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads.Tokens;

internal class CountTokensRequest
{
    [JsonIgnore]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("contents")]
    public ICollection<Content>? Contents { get; set; }
}