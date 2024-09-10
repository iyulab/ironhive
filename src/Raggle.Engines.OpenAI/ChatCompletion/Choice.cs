using System.Text.Json.Serialization;

namespace Raggle.Engines.OpenAI.ChatCompletion;

public class Choice
{
    /// <summary>
    /// "stop", "length", "content_filter", "tool_calls" 
    /// </summary>
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public AssistantMessage? Message { get; set; }

    [JsonPropertyName("logprobs")]
    public LogProbs? LogProbs { get; set; }
}

public class ChoiceDelta
{
    /// <summary>
    /// "stop", "length", "content_filter", "tool_calls" 
    /// </summary>
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("delta")]
    public AssistantMessage? Delta { get; set; }

    [JsonPropertyName("logprobs")]
    public LogProbs? LogProbs { get; set; }
}
