using System.Text.Json.Serialization;
using System.Text.Json;

namespace Raggle.Abstractions.Json;

/// <summary>
/// Unix 타임스탬프(숫자 및 문자열) 또는 날짜 문자열을 DateTime으로 변환하는 JsonConverter
/// </summary>
public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            // 숫자일 경우 Unix 타임스탬프(초)로 간주
            long unixTime = reader.GetInt64();
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            // 문자열일 경우
            var value = reader.GetString();
            if (!string.IsNullOrEmpty(value) && DateTime.TryParse(value, out var date))
            {
                return date;
            }
            else
            {
                // 파싱 실패 시 기본값(UnixEpoch) 반환
                return DateTime.UnixEpoch;
            }
        }
        else
        {
            throw new JsonException($"예상하지 못한 토큰 타입: {reader.TokenType}");
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Unix 타임스탬프 숫자로 출력합니다.
        //writer.WriteNumberValue(new DateTimeOffset(value).ToUnixTimeSeconds());

        // ISO 8601 형식의 문자열로 출력합니다.
        writer.WriteStringValue(value.ToString("o"));
    }
}
