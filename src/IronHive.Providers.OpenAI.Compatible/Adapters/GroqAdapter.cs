using System.Text.Json.Nodes;

namespace IronHive.Providers.OpenAI.Compatible.Adapters;

/// <summary>
/// Groq 제공자 어댑터입니다.
/// </summary>
/// <remarks>
/// Groq 제한사항:
/// - n=1만 지원 (다른 값은 400 에러)
/// - presence_penalty 미지원
/// - 일부 파라미터 미지원
/// </remarks>
public class GroqAdapter : BaseProviderAdapter
{
    /// <inheritdoc />
    public override CompatibleProvider Provider => CompatibleProvider.Groq;

    /// <inheritdoc />
    protected override string DefaultBaseUrl => "https://api.groq.com/openai/v1";

    /// <inheritdoc />
    public override void RemoveUnsupportedParameters(JsonObject request)
    {
        // Groq에서 지원하지 않는 파라미터 제거
        RemoveProperties(request,
            "presence_penalty",  // 미지원
            "logit_bias",        // 미지원
            "logprobs",          // 미지원
            "top_logprobs"       // 미지원
        );

        // n은 1만 지원
        if (request.ContainsKey("n"))
        {
            var n = request["n"]?.GetValue<int>();
            if (n.HasValue && n.Value != 1)
            {
                request["n"] = 1;
            }
        }

        base.RemoveUnsupportedParameters(request);
    }

    /// <inheritdoc />
    public override JsonObject TransformRequest(JsonObject request, CompatibleConfig config)
    {
        if (config is GroqConfig groqConfig)
        {
            // Groq 특수 설정 적용 (현재는 특별한 것 없음)
        }

        return base.TransformRequest(request, config);
    }
}

/// <summary>
/// Groq 특수 설정입니다.
/// </summary>
public class GroqConfig : CompatibleConfig
{
    public GroqConfig()
    {
        Provider = CompatibleProvider.Groq;
    }

    // Groq는 대부분 OpenAI 호환이므로 추가 설정 불필요
}
