using System.Text.Json.Serialization;

namespace Raggle.Connector.OpenAI.ChatCompletion.Models;

internal enum FinishReason
{
    Stop,
    Length,
    ContentFilter,
    ToolCalls
}

internal class Choice
{
    [JsonPropertyName("finish_reason")]
    internal FinishReason? FinishReason { get; set; }

    [JsonPropertyName("index")]
    internal int Index { get; set; }

    [JsonPropertyName("message")]
    internal AssistantMessage? Message { get; set; }

    [JsonPropertyName("logprobs")]
    internal LogProbs? LogProbs { get; set; }
}

internal class ChoiceDelta
{
    [JsonPropertyName("finish_reason")]
    internal FinishReason? FinishReason { get; set; }

    [JsonPropertyName("index")]
    internal int Index { get; set; }

    [JsonPropertyName("delta")]
    internal AssistantMessage? Delta { get; set; }

    [JsonPropertyName("logprobs")]
    internal LogProbs? LogProbs { get; set; }
}
