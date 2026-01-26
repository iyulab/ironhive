using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Payloads.Messages;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Base64PDFSource), "base64")]
[JsonDerivedType(typeof(TextPlainSource), "text")]
[JsonDerivedType(typeof(ContentBlockSource), "content")]
[JsonDerivedType(typeof(URLPDFSource), "url")]
internal abstract class DocumentSource
{ }

internal class Base64PDFSource : DocumentSource
{
    [JsonPropertyName("media_type")]
    public string? MediaType { get; set; } = "application/pdf";

    [JsonPropertyName("data")]
    public string? Data { get; set; }
}

internal class TextPlainSource : DocumentSource
{
    [JsonPropertyName("media_type")]
    public string? MediaType { get; set; } = "text/plain";

    [JsonPropertyName("data")]
    public string? Data { get; set; }
}

internal class ContentBlockSource : DocumentSource
{
    [JsonPropertyName("content")]
    public object? Content { get; set; }
}

internal class URLPDFSource : DocumentSource
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
