using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "role")]
[JsonDerivedType(typeof(DeveloperMessage), "developer")]
[JsonDerivedType(typeof(SystemMessage), "system")]
[JsonDerivedType(typeof(UserMessage), "user")]
[JsonDerivedType(typeof(AssistantMessage), "assistant")]
[JsonDerivedType(typeof(ToolMessage), "tool")]
internal abstract class Message 
{ }

internal class DeveloperMessage : Message
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }
}

internal class SystemMessage : Message
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }
}

internal class UserMessage : Message
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("content")]
    [JsonConverter(typeof(UserContentJsonConverter))]
    public ICollection<MessageContent> Content { get; set; } = new List<MessageContent>();
}

internal class AssistantMessage : Message
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
    public ICollection<ToolCall>? ToolCalls { get; set; }
}

/// <summary>
/// This message is result of a tool call.
/// </summary>
internal class ToolMessage : Message
{
    [JsonPropertyName("tool_call_id")]
    public required string ID { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }
}

/// <summary>
/// 일반 String을 허용하는 유저 메시지를 처리하기 위한 JsonConverter입니다.
/// </summary>
internal class UserContentJsonConverter : JsonConverter<ICollection<MessageContent>>
{
    public override ICollection<MessageContent>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            // 문자열이면 TextMessageContent 하나짜리 리스트로 변환
            string text = reader.GetString() ?? string.Empty;
            return [new TextMessageContent() { Text = text }];
        }
        else
        {
            // 이외 JSON으로 처리
            return JsonSerializer.Deserialize<List<MessageContent>>(ref reader, options);
        }
    }

    public override void Write(Utf8JsonWriter writer, ICollection<MessageContent> value, JsonSerializerOptions options)
    {
        // ICollection<MessageContent>를 JSON으로 직렬화
        JsonSerializer.Serialize(writer, value, options);
    }
}