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

    [JsonPropertyName("message")]
    public AssistantChatMessage? Message { get; set; }

    [JsonPropertyName("logprobs")]
    public ChatLogProbs? LogProbs { get; set; }
}

public class ChatChoiceDelta
{
    [JsonPropertyName("finish_reason")]
    public ChatFinishReason? FinishReason { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("delta")]
    public AssistantChatMessage? Delta { get; set; }

    [JsonPropertyName("logprobs")]
    public ChatLogProbs? LogProbs { get; set; }
}
