using System.Text.Json.Nodes;

namespace IronHive.Providers.OpenAI.Compatible.Adapters;

/// <summary>
/// OpenRouter 제공자 어댑터입니다.
/// </summary>
/// <remarks>
/// OpenRouter 특수 기능:
/// - HTTP-Referer, X-Title 헤더로 앱 귀속
/// - transforms 파라미터로 프롬프트 변환
/// - 모델별 미지원 파라미터 자동 필터링
/// - native_finish_reason 응답 필드
/// </remarks>
public class OpenRouterAdapter : BaseProviderAdapter
{
    /// <inheritdoc />
    public override CompatibleProvider Provider => CompatibleProvider.OpenRouter;

    /// <inheritdoc />
    protected override string DefaultBaseUrl => "https://openrouter.ai/api/v1";

    /// <inheritdoc />
    public override IDictionary<string, string> GetAdditionalHeaders(CompatibleConfig config)
    {
        var headers = new Dictionary<string, string>();

        if (config is OpenRouterConfig orConfig)
        {
            // HTTP-Referer: 앱 URL (리더보드 및 분석용)
            if (!string.IsNullOrEmpty(orConfig.SiteUrl))
            {
                headers["HTTP-Referer"] = orConfig.SiteUrl;
            }

            // X-Title: 앱 이름 (리더보드 표시용)
            if (!string.IsNullOrEmpty(orConfig.AppName))
            {
                headers["X-Title"] = orConfig.AppName;
            }
        }

        return headers;
    }

    /// <inheritdoc />
    public override JsonObject TransformRequest(JsonObject request, CompatibleConfig config)
    {
        if (config is OpenRouterConfig orConfig)
        {
            // Transforms 설정
            if (orConfig.Transforms != null && orConfig.Transforms.Count > 0)
            {
                var transformsArray = new JsonArray();
                foreach (var transform in orConfig.Transforms)
                {
                    transformsArray.Add(transform);
                }
                request["transforms"] = transformsArray;
            }

            // Provider preferences
            if (orConfig.ProviderPreferences != null)
            {
                var provider = new JsonObject();

                if (orConfig.ProviderPreferences.AllowFallbacks.HasValue)
                    provider["allow_fallbacks"] = orConfig.ProviderPreferences.AllowFallbacks.Value;

                if (orConfig.ProviderPreferences.RequireParameters.HasValue)
                    provider["require_parameters"] = orConfig.ProviderPreferences.RequireParameters.Value;

                if (orConfig.ProviderPreferences.Order != null && orConfig.ProviderPreferences.Order.Count > 0)
                {
                    var orderArray = new JsonArray();
                    foreach (var p in orConfig.ProviderPreferences.Order)
                    {
                        orderArray.Add(p);
                    }
                    provider["order"] = orderArray;
                }

                request["provider"] = provider;
            }

            // Route 설정 (fallback 등)
            if (!string.IsNullOrEmpty(orConfig.Route))
            {
                request["route"] = orConfig.Route;
            }
        }

        return base.TransformRequest(request, config);
    }

    /// <inheritdoc />
    public override JsonObject TransformResponse(JsonObject response)
    {
        // OpenRouter는 native_finish_reason을 별도로 제공하지만
        // 기본 finish_reason은 OpenAI 형식으로 정규화되어 있으므로 추가 처리 불필요
        return response;
    }
}

/// <summary>
/// OpenRouter 특수 설정입니다.
/// </summary>
public class OpenRouterConfig : CompatibleConfig
{
    public OpenRouterConfig()
    {
        Provider = CompatibleProvider.OpenRouter;
    }

    /// <summary>
    /// 사이트 URL (HTTP-Referer 헤더로 전송)
    /// 리더보드 및 분석에서 앱을 식별하는 데 사용됩니다.
    /// </summary>
    public string? SiteUrl { get; set; }

    /// <summary>
    /// 앱 이름 (X-Title 헤더로 전송)
    /// 리더보드에 표시되는 이름입니다.
    /// </summary>
    public string? AppName { get; set; }

    /// <summary>
    /// 프롬프트 변환 목록입니다.
    /// </summary>
    public IList<string>? Transforms { get; set; }

    /// <summary>
    /// 라우팅 전략입니다. (예: "fallback")
    /// </summary>
    public string? Route { get; set; }

    /// <summary>
    /// Provider 선호도 설정입니다.
    /// </summary>
    public OpenRouterProviderPreferences? ProviderPreferences { get; set; }
}

/// <summary>
/// OpenRouter Provider 선호도 설정입니다.
/// </summary>
public class OpenRouterProviderPreferences
{
    /// <summary>
    /// 실패 시 다른 Provider로 폴백을 허용할지 여부입니다.
    /// </summary>
    public bool? AllowFallbacks { get; set; }

    /// <summary>
    /// 요청된 파라미터를 지원하는 Provider만 사용할지 여부입니다.
    /// </summary>
    public bool? RequireParameters { get; set; }

    /// <summary>
    /// Provider 우선순위 목록입니다.
    /// </summary>
    public IList<string>? Order { get; set; }
}
