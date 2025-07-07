using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "role")]
[JsonDerivedType(typeof(DeveloperChatMessage), "developer")]
[JsonDerivedType(typeof(SystemChatMessage), "system")]
[JsonDerivedType(typeof(UserChatMessage), "user")]
[JsonDerivedType(typeof(AssistantChatMessage), "assistant")]
[JsonDerivedType(typeof(ToolChatMessage), "tool")]
public abstract class ChatMessage 
{ }

public class DeveloperChatMessage : ChatMessage
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }
}

public class SystemChatMessage : ChatMessage
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }
}

public class UserChatMessage : ChatMessage
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("content")]
    [JsonConverter(typeof(TextContentJsonConverter))]
    public ICollection<ChatMessageContent> Content { get; set; } = new List<ChatMessageContent>();
}

public class AssistantChatMessage : ChatMessage
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The content is necessary when ToolCalls is null.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("refusal")]
    public string? Refusal { get; set; }

    /// <summary>
    /// when using web search tools, urls are returned.
    /// </summary>
    [JsonPropertyName("annotations")]
    public object? Annotations { get; set; }

    /// <summary>
    /// If the audio output modality is requested
    /// </summary>
    [JsonPropertyName("audio")]
    public object? Audio { get; set; }

    /// <summary>
    /// the tools that the assistant calls.
    /// </summary>
    [JsonPropertyName("tool_calls")]
    public ICollection<OpenAIToolCall>? ToolCalls { get; set; }
}

/// <summary>
/// This message is result of a tool call.
/// </summary>
public class ToolChatMessage : ChatMessage
{
    [JsonPropertyName("tool_call_id")]
    public required string ID { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }
}

/// <summary>
/// 일반 String을 허용하는 유저 메시지를 처리하기 위한 JsonConverter입니다.
/// </summary>
public class TextContentJsonConverter : JsonConverter<ICollection<ChatMessageContent>>
{
    public override ICollection<ChatMessageContent>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            // 문자열이면 TextMessageContent 하나짜리 리스트로 변환
            string text = reader.GetString() ?? string.Empty;
            return [new TextChatMessageContent() { Text = text }];
        }
        else
        {
            // 이외 JSON으로 처리
            return JsonSerializer.Deserialize<List<ChatMessageContent>>(ref reader, options);
        }
    }

    public override void Write(Utf8JsonWriter writer, ICollection<ChatMessageContent> value, JsonSerializerOptions options)
    {
        // ICollection<MessageContent>를 JSON으로 직렬화
        JsonSerializer.Serialize(writer, value, options);
    }
}