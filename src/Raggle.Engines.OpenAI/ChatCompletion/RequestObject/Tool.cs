using System.Text.Json.Serialization;

namespace Raggle.Engines.OpenAI.ChatCompletion.ChatRequestObject;

public class Tool
{
    [JsonPropertyName("id")]
    public string? ID { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; } = "function";

    [JsonPropertyName("function")]
    public required Function Function { get; set; }
}

public class Function
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("arguments")]
    public required string Arguments { get; set; }
}
