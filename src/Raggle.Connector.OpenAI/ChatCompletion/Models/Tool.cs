using System.Text.Json.Serialization;

namespace Raggle.Connector.OpenAI.ChatCompletion.Models;

internal class Tool
{
    /// <summary>
    /// "function" only
    /// </summary>
    [JsonPropertyName("type")]
    internal string Type { get; } = "function";

    [JsonPropertyName("function")]
    internal required Function Function { get; set; }
}

internal class Function
{
    [JsonPropertyName("name")]
    internal required string Name { get; set; }

    [JsonPropertyName("description")]
    internal string? Description { get; set; }

    [JsonPropertyName("parameters")]
    internal InputSchema? Parameters { get; set; }

    /// <summary>
    /// "true" is not working, "false" is default
    /// </summary>
    [JsonPropertyName("strict")]
    internal bool Strict { get; } = false;
}

internal class InputSchema
{
    /// <summary>
    /// "object" only
    /// </summary>
    [JsonPropertyName("type")]
    internal string Type { get; } = "object";

    [JsonPropertyName("properties")]
    internal object? Properties { get; set; }

    [JsonPropertyName("required")]
    internal string[]? Required { get; set; }
}
