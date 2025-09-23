using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Responses;

internal class ResponsesPrompt
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("variables")]
    public IDictionary<string, object>? Variables { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }
}
