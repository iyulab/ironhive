using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads;

/// <summary>
/// Json 응답에 대한 객체의 기본 클래스로 사용됩니다.
/// </summary>
public abstract class JsonPayloadResponse
{
    /// <summary>
    /// 원본 JSON 응답입니다.
    /// </summary>
    [JsonIgnore]
    public JsonObject? Raw { get; set; }
}
