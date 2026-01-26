using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ResponsesWebSearchAction), "search")]
[JsonDerivedType(typeof(ResponsesWebSearchAction), "open_page")]
[JsonDerivedType(typeof(ResponsesWebSearchAction), "find")]
internal abstract class ResponsesWebSearchAction
{ }

internal class ResponsesWebSearchSearchAction : ResponsesWebSearchAction
{
    [JsonPropertyName("queries")]
    public ICollection<string>? Queries { get; set; }

    [JsonPropertyName("sources")]
    public ICollection<WebSource>? Sources { get; set; }

    internal sealed class WebSource
    {
        [JsonPropertyName("type")]
        public string Type { get; } = "url";

        [JsonPropertyName("url")]
        public required string Url { get; set; }
    }
}

internal class ResponsesWebSearchOpenPageAction : ResponsesWebSearchAction
{
    [JsonPropertyName("url")]
    public required string Url { get; set; }
}

internal class ResponsesWebSearchFindAction : ResponsesWebSearchAction
{
    [JsonPropertyName("pattern")]
    public required string Pattern { get; set; }

    [JsonPropertyName("url")]
    public required string Url { get; set; }
}
