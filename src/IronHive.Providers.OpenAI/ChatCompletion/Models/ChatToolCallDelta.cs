using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.ChatCompletion.Models;

public class ChatToolCallDelta
{
    /// <summary>
    /// only "function" supported
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "function";

    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("function")]
    public FunctionSchema? Function { get; set; }

    public class FunctionSchema
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("arguments")]
        public string? Arguments { get; set; }
    }
}
