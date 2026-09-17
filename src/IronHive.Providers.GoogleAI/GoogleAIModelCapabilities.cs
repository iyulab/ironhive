namespace IronHive.Providers.GoogleAI;

/// <summary>
/// Gemini 모델이 thinking 을 제어받는 형태입니다.
/// </summary>
public enum GoogleAIThinkingControl
{
    /// <summary>
    /// thinking 파라미터를 보내지 않습니다 — thinking 을 지원하지 않는 세대(Gemini 2.0 이하).
    /// </summary>
    None,

    /// <summary>
    /// <c>thinkingConfig.thinkingBudget</c>(토큰 예산) — Gemini 2.5 계열.
    /// https://ai.google.dev/gemini-api/docs/thinking
    /// </summary>
    Budget,

    /// <summary>
    /// <c>thinkingConfig.thinkingLevel</c>(<c>minimal</c>/<c>low</c>/<c>medium</c>/<c>high</c>) — Gemini 3 계열.
    /// </summary>
    Level,
}

/// <summary>
/// 특정 Gemini 모델(세대)이 wire 에서 받아들이는 것 — 「요청된 의도」를 「그 모델이 받는 wire」로 번역할 때
/// 생성기가 참조하는 정책입니다.
/// </summary>
/// <remarks>
/// <para>
/// 모델 세대 규칙은 vendor 지식이므로 이 provider 안에 둡니다(<c>IronHive.Abstractions</c>는 provider 중립).
/// 내장 표는 <see cref="Resolve"/>가 모델 id 의 <b>가장 긴 접두 일치</b>로 찾고, 소비자는
/// <see cref="GoogleAIConfig.ModelCapabilities"/> / <see cref="VertexAIConfig.ModelCapabilities"/>로 모델(또는
/// 접두)별 항목을 덮어써 새 모델을 코드 수정 없이 선언할 수 있습니다 — 소비자 항목이 내장 표보다 우선합니다.
/// </para>
/// <para>
/// 근거(2026-09 vendor 문서): Gemini 3.8 Flash — <c>minimal</c> thinking level 은 오류를 반환하고,
/// <c>temperature</c>/<c>topP</c>/<c>topK</c>는 설정하지 않도록 안내. Gemini 2.5 는 <c>thinkingLevel</c>이 아니라
/// <c>thinkingBudget</c>으로 제어합니다. 이 트리는 Gemini 3.8 을 실키로 실측하지 않았습니다(문서 근거).
/// </para>
/// </remarks>
public sealed record GoogleAIModelCapabilities
{
    /// <summary>
    /// thinking 제어 형태. 기본값 <see cref="GoogleAIThinkingControl.Level"/>(Gemini 3 계열).
    /// </summary>
    public GoogleAIThinkingControl ThinkingControl { get; init; } = GoogleAIThinkingControl.Level;

    /// <summary>
    /// <c>thinkingLevel: minimal</c>을 받는지 여부(<see cref="ThinkingControl"/>이 <see cref="GoogleAIThinkingControl.Level"/>일
    /// 때만 의미). <c>false</c>이면 지원되는 최저 단계 <c>low</c>로 강등합니다. 기본값 <c>true</c>.
    /// </summary>
    public bool SupportsMinimalThinking { get; init; } = true;

    /// <summary>
    /// <c>temperature</c>/<c>topP</c>/<c>topK</c>를 받는지 여부. <c>false</c>이면 요청의 값을 전달하지 않습니다
    /// (vendor 가 설정하지 말라고 안내한 모델에서 무음 무시 대신 요청 실패로 바뀌는 것을 막기 위함). 기본값 <c>true</c>.
    /// </summary>
    public bool SupportsSamplingParameters { get; init; } = true;

    /// <summary>
    /// <c>functionResponse.parts</c>에 <c>inlineData</c>(이미지·오디오 도구 결과)를 받는지 여부. <c>false</c>이면
    /// 비텍스트 도구 결과 블록을 무엇이 빠졌는지 이름 붙인 텍스트 자리표시자로 바꿔 <c>result</c>에 싣습니다 —
    /// Gemini 2.5 는 <c>400 Multimodal function responses are not supported for this model</c>로 호출 자체를
    /// 거부하고, Gemini 3 계열은 받습니다. 기본값 <c>true</c>.
    /// </summary>
    public bool SupportsMultimodalFunctionResponse { get; init; } = true;

    /// <summary>
    /// 내장 표에 없는 모델의 정책 — 최신 세대와 같다고 가정합니다.
    /// </summary>
    public static GoogleAIModelCapabilities Default { get; } = new();

    /// <summary>
    /// 내장 표. 키는 모델 id 또는 그 접두.
    /// </summary>
    public static IReadOnlyDictionary<string, GoogleAIModelCapabilities> BuiltIn { get; } = BuildBuiltIn();

    /// <summary>
    /// 모델 id 에 적용할 정책을 찾습니다. 소비자 항목(<paramref name="overrides"/>) → 내장 표 순으로, 각각
    /// 정확 일치 → 가장 긴 접두 일치로 찾고, 어디에도 없으면 <see cref="Default"/>.
    /// </summary>
    public static GoogleAIModelCapabilities Resolve(
        string model,
        IReadOnlyDictionary<string, GoogleAIModelCapabilities>? overrides = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(model);
        return Match(model, overrides) ?? Match(model, BuiltIn) ?? Default;
    }

    private static GoogleAIModelCapabilities? Match(
        string model,
        IReadOnlyDictionary<string, GoogleAIModelCapabilities>? table)
    {
        if (table is null || table.Count == 0) return null;
        if (table.TryGetValue(model, out var exact)) return exact;

        GoogleAIModelCapabilities? best = null;
        var bestLength = -1;
        foreach (var (key, value) in table)
        {
            if (key.Length > bestLength && model.StartsWith(key, StringComparison.Ordinal))
            {
                best = value;
                bestLength = key.Length;
            }
        }
        return best;
    }

    private static Dictionary<string, GoogleAIModelCapabilities> BuildBuiltIn()
    {
        // Pre-Gemini-3 generations reject inlineData inside functionResponse.parts.
        var noThinking = new GoogleAIModelCapabilities { ThinkingControl = GoogleAIThinkingControl.None, SupportsMultimodalFunctionResponse = false };
        var budget = new GoogleAIModelCapabilities { ThinkingControl = GoogleAIThinkingControl.Budget, SupportsMultimodalFunctionResponse = false };

        return new Dictionary<string, GoogleAIModelCapabilities>(StringComparer.Ordinal)
        {
            // Gemini 1.5 / 2.0 — no thinking parameter.
            ["gemini-1.5"] = noThinking,
            ["gemini-2.0"] = noThinking,
            // Gemini 2.5 — thinkingBudget; no multimodal function responses (400 on inlineData).
            ["gemini-2.5"] = budget,
            // Gemini 3.8 Flash — minimal level returns an error; sampling parameters must not be set.
            ["gemini-3.8-flash"] = new GoogleAIModelCapabilities
            {
                SupportsMinimalThinking = false,
                SupportsSamplingParameters = false,
            },
        };
    }
}
