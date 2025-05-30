using System.Text.Json.Serialization;

namespace IronHive.Providers.Ollama.Chat;

internal class Tool
{
    [JsonPropertyName("type")]
    public string Type { get; } = "function";

    [JsonPropertyName("function")]
    public FunctionTool? Function { get; set; }
}

internal class FunctionTool
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("parameters")]
    public object? Parameters { get; set; }
}

internal class ParametersSchema
{
    [JsonPropertyName("type")]
    public string Type { get; } = "object";

    [JsonPropertyName("properties")]
    public object? Properties { get; set; }

    [JsonPropertyName("required")]
    public IEnumerable<string>? Required { get; set; }
}