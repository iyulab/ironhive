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

    /// <summary>
    /// <see cref="Tools.ITool"/> 구현체(주로 <c>FunctionTool</c>)가 입력을 역직렬화하고
    /// 결과를 직렬화할 때 기본으로 사용하는 옵션입니다. <see cref="Options"/>와 분리되어 있어,
    /// 도구 입출력과 무관한 다른 소비자(예: <c>ObjectExtensions.Clone</c>)에 영향을 주지 않고
    /// 도구 전용으로만 조정할 수 있습니다.
    /// </summary>
    public static JsonSerializerOptions FunctionOptions { get; set; } = new()
    {
        PropertyNameCaseInsensitive = true,
        // 결과는 사람이 아니라 LLM이 텍스트로 소비하므로, 들여쓰기는 가독성 이득 없이 토큰만 늘립니다.
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        MaxDepth = 32,
        TypeInfoResolver = JsonSerializerOptions.Default.TypeInfoResolver,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };
}
