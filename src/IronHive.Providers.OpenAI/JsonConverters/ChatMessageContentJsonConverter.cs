using System.Text.Json;
using System.Text.Json.Serialization;
using IronHive.Providers.OpenAI.ChatCompletion.Models;

namespace IronHive.Providers.OpenAI.JsonConverters;

/// <summary>
/// 단일 문자열 메시지를 처리하기 위한 JsonConverter입니다.
/// </summary>
internal class ChatMessageContentJsonConverter : JsonConverter<ICollection<ChatMessageContent>>
{
    public override ICollection<ChatMessageContent>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            // 문자열이면 TextMessageContent 리스트로 변환
            string text = reader.GetString() ?? string.Empty;
            return [ new TextChatMessageContent() { Text = text } ];
        }
        else
        {
            // 이외 JSON으로 처리
            return JsonSerializer.Deserialize<ICollection<ChatMessageContent>>(ref reader, options);
        }
    }

    public override void Write(Utf8JsonWriter writer, ICollection<ChatMessageContent> value, JsonSerializerOptions options)
    {
        if (value.Count == 1 && value.First() is TextChatMessageContent tc)
        {
            // TextChatMessageContent 하나만 있는 경우 문자열로 출력
            writer.WriteStringValue(tc.Text);
        }
        else
        {
            // 이외 JSON 배열로 직렬화
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
