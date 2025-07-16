using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.JsonConverters;

/// <summary>
/// 단일 문자열 또는 문자열 목록을 처리하는 JsonConverter입니다.
/// </summary>
internal class EmbeddingInputJsonConverter : JsonConverter<IEnumerable<string>>
{
    public override IEnumerable<string>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            // 단일 문자열인 경우
            var str = reader.GetString();
            return str == null ? null : new[] { str };
        }
        else
        {
            // 문자열 목록인 경우
            return JsonSerializer.Deserialize<IEnumerable<string>>(ref reader, options);
        }
    }

    public override void Write(Utf8JsonWriter writer, IEnumerable<string> value, JsonSerializerOptions options)
    {
        if (value != null && value.Count() == 1)
        {
            // 단일 문자열인 경우
            writer.WriteStringValue(value.First());
        }
        else
        {
            // 문자열 목록인 경우
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}