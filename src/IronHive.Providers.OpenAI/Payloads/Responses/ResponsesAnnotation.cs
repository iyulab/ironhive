using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ResponsesFileCitation), "file_citation")]
[JsonDerivedType(typeof(ResponsesUrlCitation), "url_citation")]
[JsonDerivedType(typeof(ResponsesContainerCitation), "container_file_citation")]
[JsonDerivedType(typeof(ResponsesFilePathAnnotation), "file_path")]
public abstract class ResponsesAnnotation
{ }

public class ResponsesFileCitation : ResponsesAnnotation
{
    [JsonPropertyName("file_id")]
    public required string FileId { get; set; }

    [JsonPropertyName("filename")]
    public required string Filename { get; set; }

    [JsonPropertyName("index")]
    public required int Index { get; set; }
}

public class ResponsesUrlCitation : ResponsesAnnotation
{
    [JsonPropertyName("end_index")]
    public required int EndIndex { get; set; }

    [JsonPropertyName("start_index")]
    public required int StartIndex { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("url")]
    public required string Url { get; set; }
}

public class ResponsesContainerCitation : ResponsesAnnotation
{
    [JsonPropertyName("container_id")]
    public required string ContainerId { get; set; }

    [JsonPropertyName("end_index")]
    public required int EndIndex { get; set; }

    [JsonPropertyName("file_id")]
    public required string FileId { get; set; }

    [JsonPropertyName("filename")]
    public required string Filename { get; set; }

    [JsonPropertyName("start_index")]
    public required int StartIndex { get; set; }
}

public class ResponsesFilePathAnnotation : ResponsesAnnotation
{
    [JsonPropertyName("file_id")]
    public required string FileId { get; set; }

    [JsonPropertyName("index")]
    public required int Index { get; set; }
}
