using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Messages;

internal class ErrorContent
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName ("message")]
    public string? Message { get; set; }

    [JsonPropertyName ("details")]
    public object? Details { get; set; }
}
