using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.ChatCompletion;

public class ChatUrlAnnotation
{
    [JsonPropertyName("type")]
    public string? Type { get; } = "url_citation";

    [JsonPropertyName("url_citation")]
    public UrlCitation? Citation { get; set; }

    public sealed class UrlCitation
    {
        [JsonPropertyName("end_index")]
        public int? EndIndex { get; set; }

        [JsonPropertyName("start_index")]
        public int? StartIndex { get; set; }
        
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
