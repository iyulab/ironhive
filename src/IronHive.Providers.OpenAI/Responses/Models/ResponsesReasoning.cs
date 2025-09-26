using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Responses.Models;

internal class ResponsesReasoning
{
    /// <summary>
    /// "minimal", "low", "medium", "high"
    /// </summary>
    [JsonPropertyName("effort")]
    public ResponsesReasoningEffort Effort { get; set; }

    /// <summary>
    /// "auto" 또는 "concise", "detailed"
    /// </summary>
    [JsonPropertyName("summary")]
    public ResponsesReasoningSummary? Summary { get; set; }
}
