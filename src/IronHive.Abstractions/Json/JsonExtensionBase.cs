using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Json;

/// <summary>
/// Json 객체에 대한 확장 속성을 지원하는 기본 클래스입니다.
/// </summary>
public abstract class JsonExtensionBase
{
    /// <summary>
    /// Json 객체의 추가 속성을 설정하거나 가져옵니다.
    /// </summary>
    /// <remarks>
    /// 직렬화,역직렬화 시 이 속성에 매핑되지 않은 모든 속성이 여기에 저장됩니다(자동 처리).
    /// <see cref="JsonExtensionDataAttribute"/> 특성이 적용됩니다.
    /// <seealso href="https://learn.microsoft.com/ko-kr/dotnet/api/system.text.json.serialization.jsonextensiondataattribute?view=net-10.0"/>를 참고하세요.
    /// </remarks>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}