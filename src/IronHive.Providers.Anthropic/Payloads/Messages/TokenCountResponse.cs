using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Payloads.Messages;

internal sealed class TokenCountResponse
{
    [JsonPropertyName("input_tokens")]
    public int TokenCount { get; set; }
}
