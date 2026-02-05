using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

public class ResponsesReasoningContent
{
    /// <summary>
    /// "reasoning_text", "summary_text"
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("text")]
    public required string Text { get; set; }
}