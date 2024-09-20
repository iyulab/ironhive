using System.Text.Json.Serialization;

namespace Raggle.Engines.OpenAI.ChatCompletion;

public enum FinishReason
{
    Stop,
    Length,
    ContentFilter,
    ToolCalls
}

public class Choice
{
    [JsonPropertyName("finish_reason")]
    public FinishReason? FinishReason { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public AssistantMessage? Message { get; set; }

    [JsonPropertyName("logprobs")]
    public LogProbs? LogProbs { get; set; }
}

public class ChoiceDelta
{
    [JsonPropertyName("finish_reason")]
    public FinishReason? FinishReason { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("delta")]
    public AssistantMessage? Delta { get; set; }

    [JsonPropertyName("logprobs")]
    public LogProbs? LogProbs { get; set; }
}
