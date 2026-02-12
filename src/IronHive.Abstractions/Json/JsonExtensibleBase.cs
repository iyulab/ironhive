using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Json;

/// <summary>
/// Json 객체에 대한 확장 속성을 지원하는 기본 클래스입니다.
/// <see cref="ExtraBodyJsonConverterFactory"/>를 JsonSerializerOptions에 등록해야 동작합니다.
/// </summary>
public abstract class JsonExtensibleBase
{
    /// <summary>
    /// 직렬화 시 루트 JSON에 deep merge되고, 역직렬화 시 미매핑 속성이 수집되는 확장 속성입니다.
    /// </summary>
    [JsonIgnore]
    public JsonObject? ExtraBody { get; set; }
}
