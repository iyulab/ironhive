using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Payloads.Messages;

internal sealed class UserMetadata
{
    [JsonPropertyName("user_id")]
    public required string UserId { get; set; }
}
