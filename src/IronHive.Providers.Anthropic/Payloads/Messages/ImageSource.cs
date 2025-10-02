using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Payloads.Messages;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Base64ImageSource), "base64")]
[JsonDerivedType(typeof(UrlImageSource), "url")]
internal abstract class ImageSource
{ }

internal class Base64ImageSource : ImageSource
{
    /// <summary>
    /// "image/jpeg", "image/png", "image/gif", "image/webp" 
    /// </summary>
    [JsonPropertyName("media_type")]
    public required string MediaType { get; set; }

    /// <summary>
    /// base64 raw data
    /// </summary>
    [JsonPropertyName("data")]
    public required string Data { get; set; }
}

internal class UrlImageSource : ImageSource
{
    [JsonPropertyName("url")]
    public required string Url { get; set; }
}