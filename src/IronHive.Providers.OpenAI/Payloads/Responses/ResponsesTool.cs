using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ResponsesFunctionTool), "function")]
[JsonDerivedType(typeof(ResponsesWebSearchTool), "web_search")]
[JsonDerivedType(typeof(ResponsesCodeInterpreterTool), "code_interpreter")]
[JsonDerivedType(typeof(ResponsesImageGenerationTool), "image_generation")]
[JsonDerivedType(typeof(ResponsesCustomTool), "custom")]
internal class ResponsesTool
{ }

internal class ResponsesFunctionTool : ResponsesTool
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("parameters")]
    public required object Parameters { get; set; }

    /// <summary>
    /// 과도하게 엄격함 (기본: false).
    /// </summary>
    [JsonPropertyName("strict")]
    public bool Strict { get; set; } = false;

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

internal class ResponsesWebSearchTool : ResponsesTool
{
    [JsonPropertyName("filters")]
    public SearchFilters? Filters { get; set; }

    /// <summary> "low", "medium", "high" </summary>
    [JsonPropertyName("search_context_size")]
    public string? ContextSize { get; set; }
    
    [JsonPropertyName("user_location")]
    public SearchLocation? Location { get; set; }

    internal sealed class SearchFilters
    {
        [JsonPropertyName("allowed_domains")]
        public ICollection<string>? AllowedDomains { get; set; }
    }

    internal sealed class SearchLocation
    {
        [JsonPropertyName("city")]
        public string? City { get; set; }

        /// <summary>2자리 ISO 국가 코드 (예: "US") </summary>
        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        /// <summary>IANA 시간대 이름 (예: "America/New_York") </summary>
        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; } = "approximate";
    }
}

internal class ResponsesCodeInterpreterTool : ResponsesTool
{
    [JsonPropertyName("container")]
    public required ContainerEnvironment Container { get; set; }
    
    internal sealed class ContainerEnvironment
    {
        [JsonPropertyName("type")]
        public string Type { get; } = "auto";

        [JsonPropertyName("file_ids")]
        public ICollection<string>? FileIds { get; set; }

        [JsonPropertyName("memory_limit")]
        public string? MemoryLimit { get; set; }
    }
}

internal class ResponsesImageGenerationTool : ResponsesTool
{
    /// <summary>"transparent", "opaque", "auto" </summary>
    [JsonPropertyName("background")]
    public string? Background { get; set; }

    /// <summary>"low", "high" </summary>
    [JsonPropertyName("input_fidelity")]
    public string? InputFidelity { get; set; }

    [JsonPropertyName("input_image_mask")]
    public ImageMask? InputImageMask { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>"auto" </summary>
    [JsonPropertyName("moderation")]
    public string? Moderation { get; set; }

    [JsonPropertyName("output_compression")]
    public int? OutputCompression { get; set; }

    /// <summary>"png", "webp", "jpeg" </summary>
    [JsonPropertyName("output_format")]
    public string? OutputFormat { get; set; }

    [JsonPropertyName("partial_images")]
    public int? PartialImages { get; set; }

    /// <summary>"low", "medium", "high", "auto" </summary>
    [JsonPropertyName("quality")]
    public string? Quality { get; set; }

    /// <summary>"1024x1024", "1024x1536", "1536x1024", "auto" </summary>
    [JsonPropertyName("size")]
    public string? Size { get; set; }

    internal sealed class ImageMask
    {
        [JsonPropertyName("file_id")]
        public string? FileId { get; set; }

        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }
    }
}

internal class ResponsesCustomTool : ResponsesTool
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("format")]
    public object? Format { get; set; }
}
