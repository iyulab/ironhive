using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Compatible.ChatCompletion;

/// <summary>
/// Base class for Chat Completions request/response payloads that support an <see cref="ExtraBody"/> escape
/// hatch. Requires <see cref="ExtraBodyJsonConverterFactory"/> to be registered on the
/// <c>JsonSerializerOptions</c> used to (de)serialize the payload.
/// </summary>
public abstract class ChatCompletionPayloadBase
{
    /// <summary>
    /// On write, deep-merged into the root JSON (vendor extensions such as vLLM's <c>thinking_token_budget</c>).
    /// On read, collects properties the typed model does not recognize (e.g. <c>reasoning_content</c> emitted
    /// by Ollama/vLLM-style reasoning models), preserving their original nested position (e.g.
    /// <c>choices[0].message.reasoning_content</c>).
    /// </summary>
    [JsonIgnore]
    public JsonObject? ExtraBody { get; set; }
}

/// <summary>
/// Serializes lowercase via the snake_case <c>JsonStringEnumConverter</c> registered on the client's
/// JSON options (e.g. <see cref="Xhigh"/> → <c>"xhigh"</c>).
/// </summary>
public enum ChatReasoningEffort
{
    None,
    Minimal,
    Low,
    Medium,
    High,
    Xhigh
}

public enum ChatFinishReason
{
    Stop,
    Length,
    ContentFilter,
    ToolCalls
}

public class ChatTokenUsage
{
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

/// <summary>
/// Only the <c>json_schema</c> variant is modeled — the only one this provider ever requests. <c>strict</c>
/// is always sent as <c>false</c>: OpenAI's strict-mode constraints (all properties required,
/// <c>additionalProperties: false</c>) are not honored consistently across compatible servers.
/// </summary>
public class ChatResponseFormat
{
    [JsonPropertyName("type")]
    public string Type { get; } = "json_schema";

    [JsonPropertyName("json_schema")]
    public required JsonSchemaFormat JsonSchema { get; set; }

    public class JsonSchemaFormat
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("schema")]
        public object? Schema { get; set; }

        [JsonPropertyName("strict")]
        public bool? Strict { get; set; }
    }
}

/// <summary>Only the <c>function</c> tool type is modeled — the only one this provider ever declares.</summary>
public class ChatTool
{
    [JsonPropertyName("type")]
    public string Type { get; } = "function";

    [JsonPropertyName("function")]
    public required FunctionSchema Function { get; set; }

    public class FunctionSchema
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("parameters")]
        public object? Parameters { get; set; }

        /// <summary>Always false — strict function-parameter enforcement is not honored consistently
        /// across compatible servers.</summary>
        [JsonPropertyName("strict")]
        public bool Strict { get; } = false;
    }
}

/// <summary>
/// A tool call, used both when parsing a non-streaming response message and when replaying assistant tool-call
/// history in a request. Only <c>function</c> calls are modeled. <see cref="Type"/> must serialize on the
/// request side too — compatible servers reject assistant tool-call history that omits it.
/// </summary>
public class ChatToolCall
{
    [JsonPropertyName("type")]
    public string Type { get; } = "function";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("function")]
    public FunctionCall? Function { get; set; }

    public class FunctionCall
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("arguments")]
        public string? Arguments { get; set; }
    }
}

/// <summary>An incremental tool-call fragment on a streaming delta, keyed by <see cref="Index"/> since a
/// single chunk's arguments arrive split across many chunks.</summary>
public class ChatToolCallDelta
{
    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("function")]
    public FunctionCallDelta? Function { get; set; }

    public class FunctionCallDelta
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("arguments")]
        public string? Arguments { get; set; }
    }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextChatMessageContent), "text")]
[JsonDerivedType(typeof(ImageChatMessageContent), "image_url")]
public abstract class ChatMessageContent
{ }

public class TextChatMessageContent : ChatMessageContent
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

public class ImageChatMessageContent : ChatMessageContent
{
    [JsonPropertyName("image_url")]
    public required ImageSource ImageUrl { get; set; }

    public class ImageSource
    {
        /// <summary>Either a URL of the image or the base64-encoded data URL.</summary>
        [JsonPropertyName("url")]
        public required string Url { get; set; }

        [JsonPropertyName("detail")]
        public string? Detail { get; set; }
    }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "role")]
[JsonDerivedType(typeof(SystemChatMessage), "system")]
[JsonDerivedType(typeof(UserChatMessage), "user")]
[JsonDerivedType(typeof(AssistantChatMessage), "assistant")]
[JsonDerivedType(typeof(ToolChatMessage), "tool")]
public abstract class ChatMessage
{ }

public class SystemChatMessage : ChatMessage
{
    [JsonPropertyName("content")]
    public required string Content { get; set; }
}

public class UserChatMessage : ChatMessage
{
    [JsonPropertyName("content")]
    [JsonConverter(typeof(ChatMessageContentJsonConverter))]
    public ICollection<ChatMessageContent> Content { get; set; } = new List<ChatMessageContent>();
}

public class AssistantChatMessage : ChatMessage
{
    /// <summary>Required alongside <see cref="ToolCalls"/> by some compatible servers even when empty.</summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("tool_calls")]
    public ICollection<ChatToolCall>? ToolCalls { get; set; }
}

/// <summary>The result message of a tool call.</summary>
public class ToolChatMessage : ChatMessage
{
    [JsonPropertyName("tool_call_id")]
    public required string ToolCallId { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }
}

public class ChatChoiceMessage
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("tool_calls")]
    public ICollection<ChatToolCall>? ToolCalls { get; set; }
}

public class ChatChoiceMessageDelta
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("tool_calls")]
    public ICollection<ChatToolCallDelta>? ToolCalls { get; set; }
}

public class ChatChoice
{
    [JsonPropertyName("finish_reason")]
    public ChatFinishReason? FinishReason { get; set; }

    [JsonPropertyName("message")]
    public ChatChoiceMessage? Message { get; set; }
}

public class ChatChoiceDelta
{
    [JsonPropertyName("delta")]
    public ChatChoiceMessageDelta? Delta { get; set; }

    [JsonPropertyName("finish_reason")]
    public ChatFinishReason? FinishReason { get; set; }
}

public class ChatCompletionRequest : ChatCompletionPayloadBase
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("messages")]
    public required IEnumerable<ChatMessage> Messages { get; set; }

    [JsonPropertyName("max_completion_tokens")]
    public int? MaxCompletionTokens { get; set; }

    /// <summary>
    /// The pre-rename spelling of the output limit. Only populated when the caller selects it via
    /// <see cref="TokenLimitParameter"/>; null is omitted from the payload, so a server never sees a
    /// name it was not asked to receive.
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }

    [JsonPropertyName("top_p")]
    public float? TopP { get; set; }

    /// <summary>Not part of the OpenAI Chat Completions schema; llama.cpp/vLLM-style servers accept it.
    /// Servers that do not recognize it ignore it.</summary>
    [JsonPropertyName("top_k")]
    public int? TopK { get; set; }

    [JsonPropertyName("stop")]
    public ICollection<string>? Stop { get; set; }

    [JsonPropertyName("response_format")]
    public ChatResponseFormat? ResponseFormat { get; set; }

    [JsonPropertyName("tools")]
    public IEnumerable<ChatTool>? Tools { get; set; }

    /// <summary>OpenAI-compatible tool_choice — a bare string ("none"/"auto"/"required") or
    /// {"type":"function","function":{"name":...}}, or omitted entirely for the server's own
    /// default. Built from <see cref="IronHive.Abstractions.Messages.MessageToolChoice"/> by
    /// <see cref="ChatCompletionMessageGenerator.BuildToolChoice"/>.</summary>
    [JsonPropertyName("tool_choice")]
    public JsonNode? ToolChoice { get; set; }

    /// <summary>o-series/gpt-5-style reasoning effort. Most compatible servers ignore an unrecognized value
    /// silently; the vendor-specific overrides in <see cref="ChatCompletionPayloadBase.ExtraBody"/> carry the
    /// actual signal for reasoning-capable open-weight models.</summary>
    [JsonPropertyName("reasoning_effort")]
    public ChatReasoningEffort? ReasoningEffort { get; set; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    [JsonPropertyName("stream_options")]
    public ChatCompletionStreamOptions? StreamOptions { get; set; }
}

public class ChatCompletionStreamOptions
{
    [JsonPropertyName("include_usage")]
    public bool? IncludeUsage { get; set; }
}

public class ChatCompletionResponse : ChatCompletionPayloadBase
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("choices")]
    public IEnumerable<ChatChoice>? Choices { get; set; }

    [JsonPropertyName("usage")]
    public ChatTokenUsage? Usage { get; set; }
}

public class StreamingChatCompletionResponse : ChatCompletionPayloadBase
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("choices")]
    public IEnumerable<ChatChoiceDelta>? Choices { get; set; }

    [JsonPropertyName("usage")]
    public ChatTokenUsage? Usage { get; set; }
}
