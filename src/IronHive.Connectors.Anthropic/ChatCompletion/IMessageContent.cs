using System.Text.Json.Serialization;

namespace IronHive.Connectors.Anthropic.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextMessageContent), "text")]
[JsonDerivedType(typeof(ImageMessageContent), "image")]
[JsonDerivedType(typeof(ToolUseMessageContent), "tool_use")]
[JsonDerivedType(typeof(ToolResultMessageContent), "tool_result")]
[JsonDerivedType(typeof(DocumentMessageContent), "document")]
[JsonDerivedType(typeof(ThinkingMessageContent), "thinking")]
[JsonDerivedType(typeof(RedactedThinkingMessageContent), "redacted_thinking")]
internal interface IMessageContent
{ }

internal abstract class MessageContentBase : IMessageContent
{
    [JsonPropertyName("cache_control")]
    public CacheControl? CacheControl { get; set; }
}

internal class TextMessageContent : MessageContentBase
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }

    [JsonPropertyName("ciations")]
    public object? Citations { get; set; }
}

internal class ImageMessageContent : MessageContentBase
{
    [JsonPropertyName("source")]
    public required ImageSource Source { get; set; }
}

internal class ToolUseMessageContent : MessageContentBase
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("input")]
    public object? Input { get; set; }
}

internal class ToolResultMessageContent : MessageContentBase
{
    [JsonPropertyName("tool_use_id")]
    public string? ToolUseId { get; set; }

    [JsonPropertyName("is_error")]
    public bool IsError { get; set; } = false;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

internal class DocumentMessageContent : MessageContentBase
{
    [JsonPropertyName("source")]
    public object? Source { get; set; }

    [JsonPropertyName("citations")]
    public object? Citations { get; set; }

    [JsonPropertyName("context")]
    public string? Context { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}

internal class ThinkingMessageContent : IMessageContent
{
    /// <summary>
    /// 이 서명은 API에 블록을 다시 전달할 때 유효성을 확인하는 데 사용됩니다.
    /// 이는 해당 블록이 Claude에 의해 생성되었음을 검증하는 암호화된 토큰입니다.
    /// </summary>
    [JsonPropertyName("signature")]
    public required string Signature { get; set; }

    [JsonPropertyName("thinking")]
    public required string Thinking { get; set; }
}

internal class RedactedThinkingMessageContent : IMessageContent
{
    /// <summary>
    /// Claude가 내부 추론을 수행하는 과정 중, 보안상 민감하다고 판단되는 정보는 이 블록에 암호화되어 들어갑니다.
    /// 사람은 읽을 수 없지만 Claude는 나중에 이 블록을 기반으로 추론을 계속 이어갈 수 있습니다.
    /// </summary>
    [JsonPropertyName("data")]
    public required string Data { get; set; }
}
