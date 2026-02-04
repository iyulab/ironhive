using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// OpenAI 호환 API 벤더를 식별합니다.
/// </summary>
public enum OpenAICompatibility
{
    /// <summary>표준 OpenAI API (기본값)</summary>
    Default = 0,
    /// <summary>xAI Grok API</summary>
    XAI = 1,
    /// <summary>Azure OpenAI Service</summary>
    Azure = 2,
}

/// <summary>
/// OpenAI에 대한 설정 클래스입니다.
/// </summary>
public class OpenAIConfig
{
    /// <summary>
    /// OpenAI API의 기본 URL을 가져오거나 설정합니다.
    /// (Default: "https://api.openai.com/v1/")
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// OpenAI API 키를 가져오거나 설정합니다.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// OpenAI 계정의 조직 ID를 가져오거나 설정합니다.
    /// 이 옵션은 선택 사항이며 청구할 조직을 지정하는 데 사용할 수 있습니다.
    /// </summary>
    public string Organization { get; set; } = string.Empty;

    /// <summary>
    /// OpenAI 프로젝트 ID를 가져오거나 설정합니다.
    /// 이 옵션은 선택 사항이며 특정 프로젝트에 대한 요청을 지정하는 데 사용할 수 있습니다.
    /// </summary>
    public string Project { get; set; } = string.Empty;

    /// <summary>
    /// OpenAI 호환 벤더를 식별합니다.
    /// xAI, Azure 등 벤더별 요청 빌딩 차이를 처리하기 위해 사용됩니다.
    /// </summary>
    public OpenAICompatibility Compatibility { get; set; } = OpenAICompatibility.Default;

    /// <summary>
    /// 요청/응답의 JSON 직렬화 및 역직렬화에 사용되는 옵션을 가져오거나 설정합니다.
    /// </summary>
    [JsonIgnore]
    public JsonSerializerOptions JsonOptions { get; set; } = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        //PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        AllowOutOfOrderMetadataProperties = true, // 다형성 역직렬화시 첫번째 속성이 아닌 경우에도 인식(성능 저하 있음)
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)
        },
    };
}