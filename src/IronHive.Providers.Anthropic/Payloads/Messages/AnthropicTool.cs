using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Payloads.Messages;

/// <summary>
/// Custom tool only, not use other tools
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(CustomAnthropicTool), "custom")]
[JsonDerivedType(typeof(WebSearchAnthropicTool), "web_search_20250305")]
internal abstract class AnthropicTool
{
    [JsonPropertyName("cache_control")]
    public CacheControl? CacheControl { get; set; }
}

internal class CustomAnthropicTool : AnthropicTool
{
    [JsonPropertyName("input_schema")]
    public required object InputSchema { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

internal class WebSearchAnthropicTool : AnthropicTool
{
    [JsonPropertyName("name")]
    public string Name { get; } = "web_search";

    [JsonPropertyName("allowed_domains")]
    public ICollection<string>? AllowedDomains { get; set; }

    [JsonPropertyName("blocked_domains")]
    public ICollection<string>? BlockedDomains { get; set; }

    [JsonPropertyName("max_uses")]
    public int? MaxUses { get; set; }

    [JsonPropertyName("user_location")]
    public UserLocation? Location { get; set; }

    public sealed class UserLocation
    {
        [JsonPropertyName("type")]
        public string Type { get; } = "approximate";

        [JsonPropertyName("city")]
        public string? City { get; set; }

        /// <summary>
        /// two letter ISO country code, e.g., "US"
        /// </summary>
        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        /// <summary>
        /// IANA Timezone format, e.g., "America/Los_Angeles"
        /// </summary>
        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }
    }
}