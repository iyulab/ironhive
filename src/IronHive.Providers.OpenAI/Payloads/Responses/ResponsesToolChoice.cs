using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ResponsesAllowedToolsChoice), "allowed_tools")]
[JsonDerivedType(typeof(ResponsesWebSearchToolChoice), "web_search")]
[JsonDerivedType(typeof(ResponsesCodeInterpreterToolChoice), "code_interpreter")]
[JsonDerivedType(typeof(ResponsesImageGenerationToolChoice), "image_generation")]
[JsonDerivedType(typeof(ResponsesFunctionToolChoice), "function")]
[JsonDerivedType(typeof(ResponsesCustomToolChoice), "custom")]
public abstract class ResponsesToolChoice
{ }

public class ResponsesAllowedToolsChoice : ResponsesToolChoice
{
    /// <summary>
    /// "auto" or "required"
    /// </summary>
    [JsonPropertyName("mode")]
    public required string Mode { get; set; }

    [JsonPropertyName("tools")]
    public required ICollection<ResponsesTool> Tools { get; set; }
}

public class ResponsesWebSearchToolChoice : ResponsesToolChoice
{ }

public class ResponsesCodeInterpreterToolChoice : ResponsesToolChoice
{ }

public class ResponsesImageGenerationToolChoice : ResponsesToolChoice
{ }

public class ResponsesFunctionToolChoice : ResponsesToolChoice
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}

public class ResponsesCustomToolChoice : ResponsesToolChoice
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}
