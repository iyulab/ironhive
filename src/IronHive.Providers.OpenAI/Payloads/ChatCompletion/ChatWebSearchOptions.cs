using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.ChatCompletion;

public class ChatWebSearchOptions
{
    /// <summary>
    /// "low", "medium", "high" - default is "medium"
    /// </summary>
    [JsonPropertyName("search_context_size")]
    public string? ContextSize { get; set; }

    [JsonPropertyName("user_location")]
    public WebSearchLocation? Location { get; set; }
}

public class WebSearchLocation
{
    [JsonPropertyName("approximate")]
    public string? City { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; } = "approximate";

    public sealed class ApproximateLocation
    {        
        [JsonPropertyName("city")]
        public string? City { get; set; }

        /// <summary> Two letter ISO country code </summary>
        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        /// <summary> IANA Timezone name </summary>
        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }
    }
}
