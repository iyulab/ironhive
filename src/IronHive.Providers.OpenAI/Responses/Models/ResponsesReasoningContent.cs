using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Responses.Models;

internal class ResponsesReasoningContent
{
    /// <summary>
    /// "reasoning_text", "summary_text"
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("text")]
    public required string Text { get; set; }
}