using System.Text.Json.Serialization;
using System.Text.Json;

namespace Raggle.Driver.OpenAI.Converters;

/// <summary>
/// Converts Unix time (seconds since epoch) to DateTime.
/// </summary>
public class UnixTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var unixTime = reader.GetInt64();
        return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var unixTime = new DateTimeOffset(value).ToUnixTimeSeconds();
        writer.WriteNumberValue(unixTime);
    }
}
