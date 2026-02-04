using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads.GenerateContent;

internal class GenerateContentRequest
{
    [JsonIgnore]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// 대화의 턴(메시지) 목록. 단발성 요청은 1개, 멀티턴은 히스토리를 포함합니다.
    /// </summary>
    [JsonPropertyName("contents")]
    public ICollection<Content> Contents { get; set; } = [];

    /// <summary>
    /// 모델이 사용할 수 있는 도구 목록(함수 호출, 코드 실행, 구글 검색/URL 컨텍스트 등).
    /// </summary>
    [JsonPropertyName("tools")]
    public ICollection<Tool>? Tools { get; set; }

    [JsonPropertyName("toolConfig")]
    public ToolConfig? ToolConfig { get; set; }

    /// <summary>
    /// 유해 컨텐츠 차단 임계치 설정. 카테고리별 0~1개만 지정.
    /// </summary>
    [JsonPropertyName("safetySettings")]
    public ICollection<SafetySetting>? SafetySettings { get; set; }

    /// <summary>
    /// 시스템 인스트럭션(개발자 시스템 프롬프트). 현재 텍스트만.
    /// </summary>
    [JsonPropertyName("systemInstruction")]
    public Content? SystemInstruction { get; set; }

    /// <summary>
    /// 생성 제어 파라미터(샘플링, 토큰 한도, JSON 모드 등).
    /// </summary>
    [JsonPropertyName("generationConfig")]
    public GenerationConfig? GenerationConfig { get; set; }

    /// <summary>
    /// 명시적 컨텍스트 캐시 사용 시 캐시 리소스 이름. 예: cachedContents/{id}
    /// </summary>
    [JsonPropertyName("cachedContent")]
    public string? CachedContent { get; set; }
}