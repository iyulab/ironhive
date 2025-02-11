using System.Text.Json.Serialization;

namespace Raggle.Driver.OpenAI.ChatCompletion;

internal class Tool
{
    /// <summary>
    /// "function" only
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "function";

    [JsonPropertyName("function")]
    public required Function Function { get; set; }
}

internal class Function
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("parameters")]
    public object? Parameters { get; set; }

    /// <summary>
    /// "true" is not working, "false" is default
    /// </summary>
    [JsonPropertyName("strict")]
    public bool Strict { get; } = false;
}
