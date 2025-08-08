using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.ChatCompletion;

public enum ChatFinishReason
{
    Stop,
    Length,
    ContentFilter,
    ToolCalls
}

public class ChatChoice
{
    [JsonPropertyName("finish_reason")]
    public ChatFinishReason? FinishReason { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("logprobs")]
    public ChatLogProbs? LogProbs { get; set; }

    [JsonPropertyName("message")]
    public ChatChoiceMessage? Message { get; set; }
}

public class ChatChoiceDelta
{
    [JsonPropertyName("delta")]
    public ChatChoiceMessageDelta? Delta { get; set; }

    [JsonPropertyName("finish_reason")]
    public ChatFinishReason? FinishReason { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("logprobs")]
    public ChatLogProbs? LogProbs { get; set; }
}
