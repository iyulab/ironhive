using IronHive.Providers.OpenAI.ChatCompletion;
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Responses;

internal class ResponsesReasoning
{
    /// <summary>
    /// "minimal", "low", "medium", "high"
    /// </summary>
    [JsonPropertyName("effort")]
    public OpenAIReasoningEffort Effort { get; set; }

    /// <summary>
    /// "auto" 또는 "concise", "detailed"
    /// </summary>
    [JsonPropertyName("summary")]
    public OpenAIReasoningSummary? Summary { get; set; }
}
