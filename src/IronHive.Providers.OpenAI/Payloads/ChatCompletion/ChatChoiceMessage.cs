using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.ChatCompletion;

public class ChatChoiceMessage
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("refusal")]
    public string? Refusal { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    /// <summary>
    /// when using web search tools, urls are returned.
    /// </summary>
    [JsonPropertyName("annotations")]
    public ICollection<ChatUrlAnnotation>? Annotations { get; set; }

    /// <summary>
    /// If the audio output modality is requested
    /// </summary>
    [JsonPropertyName("audio")]
    public ChatAudioContent? Audio { get; set; }

    /// <summary>
    /// the tools that the assistant calls.
    /// </summary>
    [JsonPropertyName("tool_calls")]
    public ICollection<ChatToolCall>? ToolCalls { get; set; }

    /// <summary>
    /// Chain-of-Thought reasoning content (DeepSeek, xAI grok-3-mini, LM Studio)
    /// </summary>
    [JsonPropertyName("reasoning_content")]
    public string? ReasoningContent { get; set; }

    /// <summary>
    /// Reasoning content (vLLM, LM Studio gpt-oss)
    /// </summary>
    [JsonPropertyName("reasoning")]
    public string? Reasoning { get; set; }

    /// <summary>
    /// Thinking content (Ollama)
    /// </summary>
    [JsonPropertyName("thinking")]
    public string? Thinking { get; set; }
}

public class ChatChoiceMessageDelta
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("refusal")]
    public string? Refusal { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    /// <summary>
    /// the tools that the assistant calls.
    /// </summary>
    [JsonPropertyName("tool_calls")]
    public ICollection<ChatToolCallDelta>? ToolCalls { get; set; }

    /// <summary>
    /// Chain-of-Thought reasoning content (DeepSeek, xAI grok-3-mini, LM Studio)
    /// </summary>
    [JsonPropertyName("reasoning_content")]
    public string? ReasoningContent { get; set; }

    /// <summary>
    /// Reasoning content (vLLM, LM Studio gpt-oss)
    /// </summary>
    [JsonPropertyName("reasoning")]
    public string? Reasoning { get; set; }

    /// <summary>
    /// Thinking content (Ollama)
    /// </summary>
    [JsonPropertyName("thinking")]
    public string? Thinking { get; set; }
}