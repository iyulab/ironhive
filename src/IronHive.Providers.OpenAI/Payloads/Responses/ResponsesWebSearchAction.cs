using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ResponsesWebSearchSearchAction), "search")]
[JsonDerivedType(typeof(ResponsesWebSearchOpenPageAction), "open_page")]
[JsonDerivedType(typeof(ResponsesWebSearchFindAction), "find")]
public abstract class ResponsesWebSearchAction
{ }

public class ResponsesWebSearchSearchAction : ResponsesWebSearchAction
{
    [JsonPropertyName("queries")]
    public ICollection<string>? Queries { get; set; }

    [JsonPropertyName("sources")]
    public ICollection<WebSource>? Sources { get; set; }

    public sealed class WebSource
    {
        [JsonPropertyName("type")]
        public string Type { get; } = "url";

        [JsonPropertyName("url")]
        public required string Url { get; set; }
    }
}

public class ResponsesWebSearchOpenPageAction : ResponsesWebSearchAction
{
    [JsonPropertyName("url")]
    public required string Url { get; set; }
}

public class ResponsesWebSearchFindAction : ResponsesWebSearchAction
{
    [JsonPropertyName("pattern")]
    public required string Pattern { get; set; }

    [JsonPropertyName("url")]
    public required string Url { get; set; }
}
