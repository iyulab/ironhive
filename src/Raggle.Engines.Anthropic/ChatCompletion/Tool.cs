using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Raggle.Engines.Anthropic.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AutoToolChoice), "auto")]
[JsonDerivedType(typeof(AnyToolChoice), "any")]
[JsonDerivedType(typeof(ManualToolChoice), "tool")]
public abstract class ToolChoice { }

public class AutoToolChoice : ToolChoice { }

public class AnyToolChoice : ToolChoice { }

public class ManualToolChoice : ToolChoice
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}

public class Tool
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("input_schema")]
    public required InputSchema InputSchema { get; set; }

    [JsonPropertyName("cache_control")]
    public object? CacheControl { get; set; }
}

public class InputSchema
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("properties")]
    public object? Properties { get; set; }
}
