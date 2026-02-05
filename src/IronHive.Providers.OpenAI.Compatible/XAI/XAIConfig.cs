using IronHive.Providers.OpenAI;

namespace IronHive.Providers.OpenAI.Compatible.XAI;

/// <summary>
/// xAI (Grok) 서비스 설정입니다.
/// </summary>
/// <remarks>
/// xAI 특수 기능:
/// - reasoning_effort 파라미터 지원
/// - store, previous_response_id 파라미터
/// - Server-side tools: web_search, x_search, code_execution
/// - search_enabled, search_parameters
/// </remarks>
public class XAIConfig : CompatibleConfig
{
    private const string DefaultBaseUrl = "https://api.x.ai/v1";

    /// <summary>
    /// 실시간 웹 검색을 활성화합니다.
    /// </summary>
    public bool EnableSearch { get; set; }

    /// <summary>
    /// 검색 파라미터 설정입니다.
    /// </summary>
    public XAISearchParameters? SearchParameters { get; set; }

    /// <summary>
    /// 생성 결과를 xAI에 저장할지 여부입니다.
    /// </summary>
    public bool? Store { get; set; }

    /// <summary>
    /// 이전 응답 ID로 대화를 연속합니다.
    /// </summary>
    public string? PreviousResponseId { get; set; }

    /// <summary>
    /// OpenAIConfig로 변환합니다.
    /// </summary>
    public override OpenAIConfig ToOpenAI()
    {
        return new OpenAIConfig
        {
            BaseUrl = DefaultBaseUrl,
            ApiKey = ApiKey ?? string.Empty,
        };
    }
}

/// <summary>
/// xAI 검색 파라미터입니다.
/// </summary>
public class XAISearchParameters
{
    /// <summary>
    /// 최대 검색 결과 수입니다.
    /// </summary>
    public int? MaxResults { get; set; }

    /// <summary>
    /// 인용 정보를 포함할지 여부입니다.
    /// </summary>
    public bool? IncludeCitations { get; set; }

    /// <summary>
    /// 검색 타임아웃 (초)입니다.
    /// </summary>
    public int? TimeoutSeconds { get; set; }
}
