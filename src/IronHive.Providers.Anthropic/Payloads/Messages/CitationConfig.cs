using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Payloads.Messages;

internal class CitationConfig
{
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }
}
