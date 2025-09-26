using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Json;

/// <summary>
/// JSON 직렬화 및 역직렬화에 사용되는 기본 옵션을 제공하는 클래스입니다.
/// </summary>
public static class JsonDefaultOptions
{
    /// <summary>
    /// 기본 JsonSerializerOptions입니다.
    /// </summary>
    public static JsonSerializerOptions Options { get; set; } = new()
    {
        PropertyNameCaseInsensitive = true,                                 // 대소문자 구분 안함
        WriteIndented = true,                                               // 들여쓰기 사용
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,              // 이스케이프를 허용합니다. (ex: <, >, &, ', " 등)
        NumberHandling = JsonNumberHandling.AllowReadingFromString,         // 문자열에서 숫자 읽기 허용
        MaxDepth = 32,                                                      // 오브젝트 최대 깊이
        TypeInfoResolver = JsonSerializerOptions.Default.TypeInfoResolver,
        Converters =
        {
            new JsonStringEnumConverter()                                   // Enum을 string로 변환
        }
    };

    /// <summary>
    /// 클론 or 복사할 때 사용되는 옵션입니다.
    /// </summary>
    public static JsonSerializerOptions CopyOptions => new()
    {
        WriteIndented = false,
        IncludeFields = true,
        PropertyNameCaseInsensitive = true
    };
}
