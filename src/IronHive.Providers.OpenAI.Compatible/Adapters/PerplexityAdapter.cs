using System.Text.Json.Nodes;

namespace IronHive.Providers.OpenAI.Compatible.Adapters;

/// <summary>
/// Perplexity 제공자 어댑터입니다.
/// </summary>
/// <remarks>
/// Perplexity는 검색 기반 AI 서비스로, 실시간 웹 정보를 활용합니다.
/// 모델: sonar-deep-research, sonar-reasoning-pro, sonar-reasoning, sonar-pro, sonar, r1-1776
/// </remarks>
public class PerplexityAdapter : BaseProviderAdapter
{
    /// <inheritdoc />
    public override CompatibleProvider Provider => CompatibleProvider.Perplexity;

    /// <inheritdoc />
    protected override string DefaultBaseUrl => "https://api.perplexity.ai";

    /// <inheritdoc />
    public override void RemoveUnsupportedParameters(JsonObject request)
    {
        // Perplexity에서 지원하지 않을 수 있는 파라미터
        RemoveProperties(request,
            "logit_bias",
            "logprobs",
            "top_logprobs",
            "n"  // n=1만 지원할 수 있음
        );

        base.RemoveUnsupportedParameters(request);
    }

    /// <inheritdoc />
    public override JsonObject TransformRequest(JsonObject request, CompatibleConfig config)
    {
        if (config is PerplexityConfig pplxConfig)
        {
            // Perplexity 특수 설정
            // 검색 관련 설정은 모델에 내장되어 있음
        }

        return base.TransformRequest(request, config);
    }
}

/// <summary>
/// Perplexity 특수 설정입니다.
/// </summary>
public class PerplexityConfig : CompatibleConfig
{
    public PerplexityConfig()
    {
        Provider = CompatibleProvider.Perplexity;
    }

    // Perplexity의 검색 기능은 모델에 내장되어 있음
}
