using System.Text.Json.Nodes;
using IronHive.Providers.OpenAI;

namespace IronHive.Providers.OpenAI.Compatible.Adapters;

/// <summary>
/// xAI (Grok) 제공자 어댑터입니다.
/// </summary>
/// <remarks>
/// xAI 특수 기능:
/// - reasoning_effort 파라미터 지원
/// - store, previous_response_id 파라미터
/// - Server-side tools: web_search, x_search, code_execution
/// - search_enabled, search_parameters
/// </remarks>
public class xAIAdapter : BaseProviderAdapter
{
    /// <inheritdoc />
    public override CompatibleProvider Provider => CompatibleProvider.xAI;

    /// <inheritdoc />
    protected override string DefaultBaseUrl => "https://api.x.ai/v1";

    /// <inheritdoc />
    public override JsonObject TransformRequest(JsonObject request, CompatibleConfig config)
    {
        // xAI 특수 설정 적용
        if (config is xAIConfig xaiConfig)
        {
            // 검색 옵션 설정
            if (xaiConfig.EnableSearch)
            {
                request["search_enabled"] = true;

                if (xaiConfig.SearchParameters != null)
                {
                    var searchParams = new JsonObject();

                    if (xaiConfig.SearchParameters.MaxResults.HasValue)
                        searchParams["max_search_results"] = xaiConfig.SearchParameters.MaxResults.Value;

                    if (xaiConfig.SearchParameters.IncludeCitations.HasValue)
                        searchParams["include_citations"] = xaiConfig.SearchParameters.IncludeCitations.Value;

                    if (xaiConfig.SearchParameters.TimeoutSeconds.HasValue)
                        searchParams["search_timeout"] = xaiConfig.SearchParameters.TimeoutSeconds.Value;

                    request["search_parameters"] = searchParams;
                }
            }

            // Store 옵션
            if (xaiConfig.Store.HasValue)
            {
                request["store"] = xaiConfig.Store.Value;
            }

            // Previous response ID (대화 연속성)
            if (!string.IsNullOrEmpty(xaiConfig.PreviousResponseId))
            {
                request["previous_response_id"] = xaiConfig.PreviousResponseId;
            }
        }

        return base.TransformRequest(request, config);
    }

    /// <inheritdoc />
    public override JsonObject TransformResponse(JsonObject response)
    {
        // xAI는 OpenAI 형식과 대부분 호환되므로 추가 변환 불필요
        return response;
    }
}

/// <summary>
/// xAI 특수 설정입니다.
/// </summary>
public class xAIConfig : CompatibleConfig
{
    /// <summary>
    /// xAI API를 위한 기본 설정으로 초기화합니다.
    /// </summary>
    public xAIConfig()
    {
        Provider = CompatibleProvider.xAI;
        BaseUrl = "https://api.x.ai/v1";
    }

    /// <summary>
    /// 실시간 웹 검색을 활성화합니다.
    /// </summary>
    public bool EnableSearch { get; set; }

    /// <summary>
    /// 검색 파라미터 설정입니다.
    /// </summary>
    public xAISearchParameters? SearchParameters { get; set; }

    /// <summary>
    /// 생성 결과를 xAI에 저장할지 여부입니다.
    /// </summary>
    public bool? Store { get; set; }

    /// <summary>
    /// 이전 응답 ID로 대화를 연속합니다.
    /// </summary>
    public string? PreviousResponseId { get; set; }

    /// <summary>
    /// xAI의 Responses API 사용을 위해 OpenAIConfig로 변환합니다.
    /// </summary>
    public OpenAIConfig ToOpenAIConfig()
    {
        return new OpenAIConfig
        {
            ApiKey = ApiKey,
            BaseUrl = BaseUrl ?? "https://api.x.ai/v1",
            DefaultHeaders = DefaultHeaders
        };
    }
}

/// <summary>
/// xAI 검색 파라미터입니다.
/// </summary>
public class xAISearchParameters
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
