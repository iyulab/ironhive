namespace IronHive.Providers.Anthropic;

/// <summary>
/// Claude 모델의 thinking 파라미터 형태입니다.
/// </summary>
public enum AnthropicThinkingStyle
{
    /// <summary>
    /// <c>thinking: { type: "enabled", budget_tokens }</c> — Claude 4.x 계열.
    /// <see cref="IronHive.Abstractions.Messages.MessageThinkingEffort"/>를 토큰 예산으로 번역합니다.
    /// </summary>
    Budget,

    /// <summary>
    /// <c>thinking: { type: "adaptive" }</c> — Claude 5 계열.
    /// https://platform.claude.com/docs/en/build-with-claude/adaptive-thinking
    /// </summary>
    Adaptive,
}

/// <summary>
/// 특정 Claude 모델(세대)이 wire 에서 받아들이는 것 — 「요청된 의도」를 「그 모델이 받는 wire」로 번역할 때
/// 생성기가 참조하는 정책입니다.
/// </summary>
/// <remarks>
/// <para>
/// 모델 세대 규칙은 vendor 지식이므로 이 provider 안에 둡니다(<c>IronHive.Abstractions</c>는 provider 중립).
/// 내장 표는 <see cref="Resolve"/>가 모델 id 의 <b>가장 긴 접두 일치</b>로 찾고, 소비자는
/// <see cref="AnthropicConfig.ModelCapabilities"/>로 모델(또는 접두)별 항목을 덮어써 새 모델을 코드 수정 없이
/// 선언할 수 있습니다 — 소비자 항목이 내장 표보다 우선합니다.
/// </para>
/// <para>
/// 근거(2026-09 vendor 문서): Claude Fable 5.1 / Mythos 5.1 마이그레이션 가이드 — <c>tool_choice</c>
/// <c>any</c>/<c>tool</c>은 400 <c>invalid_request_error</c>, <c>auto</c>·<c>none</c>만 지원. 처방은
/// <c>auto</c> + 명시 지시. 이 트리는 두 모델을 실키로 실측하지 않았습니다(문서 근거).
/// </para>
/// </remarks>
public sealed record AnthropicModelCapabilities
{
    /// <summary>
    /// <c>tool_choice: any</c> / <c>tool_choice: tool</c>(도구 호출 강제)를 받는지 여부. <c>false</c>이면 생성기가
    /// <c>auto</c>로 강등하고 시스템 프롬프트 끝에 도구 호출 지시를 덧붙입니다 — vendor 마이그레이션 가이드의
    /// 처방과 같은 형태입니다. 기본값 <c>true</c>.
    /// </summary>
    public bool SupportsForcedToolChoice { get; init; } = true;

    /// <summary>
    /// thinking 파라미터 형태. 기본값 <see cref="AnthropicThinkingStyle.Adaptive"/>(최신 세대).
    /// </summary>
    public AnthropicThinkingStyle ThinkingStyle { get; init; } = AnthropicThinkingStyle.Adaptive;

    /// <summary>
    /// <c>thinking: {type: "disabled"}</c>을 받는지 여부(<see cref="AnthropicThinkingStyle.Adaptive"/> 세대에서만 의미) —
    /// 요청의 <see cref="IronHive.Abstractions.Messages.MessageThinkingEffort.None"/>을 번역할 때 참조합니다.
    /// <c>false</c>이면 끌 수 없는 모델로 보고 <c>output_config.effort: low</c>를 보냅니다(Claude Fable 은 disabled 가 400,
    /// Claude Opus 5 는 disabled 에서 도구 호출이 본문 텍스트로 새는 실패 형태가 있어 vendor 가 낮은 effort 를 권함).
    /// 기본값 <c>false</c> — 모르는 새 모델에 disabled 를 보내 400 이 되는 것보다 낮은 effort 가 안전합니다.
    /// </summary>
    public bool SupportsDisabledThinking { get; init; }

    /// <summary>
    /// <c>output_config.effort: xhigh</c>를 받는지 여부(<see cref="AnthropicThinkingStyle.Adaptive"/> 세대). <c>false</c>이면
    /// <see cref="IronHive.Abstractions.Messages.MessageThinkingEffort.XHigh"/>를 <c>high</c>로 보냅니다 — <c>xhigh</c>는
    /// Claude Opus 4.7 에서 도입돼 4.6 세대는 받지 않습니다. 기본값 <c>true</c>.
    /// </summary>
    public bool SupportsXHighEffort { get; init; } = true;

    /// <summary>
    /// 내장 표에 없는 모델의 정책 — 최신 세대와 같다고 가정합니다.
    /// </summary>
    public static AnthropicModelCapabilities Default { get; } = new();

    /// <summary>
    /// 내장 표. 키는 모델 id 또는 그 접두(날짜 접미사 없이).
    /// </summary>
    public static IReadOnlyDictionary<string, AnthropicModelCapabilities> BuiltIn { get; } = BuildBuiltIn();

    /// <summary>
    /// 모델 id 에 적용할 정책을 찾습니다. 소비자 항목(<paramref name="overrides"/>) → 내장 표 순으로, 각각
    /// 정확 일치 → 가장 긴 접두 일치로 찾고, 어디에도 없으면 <see cref="Default"/>.
    /// </summary>
    public static AnthropicModelCapabilities Resolve(
        string model,
        IReadOnlyDictionary<string, AnthropicModelCapabilities>? overrides = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(model);
        return Match(model, overrides) ?? Match(model, BuiltIn) ?? Default;
    }

    private static AnthropicModelCapabilities? Match(
        string model,
        IReadOnlyDictionary<string, AnthropicModelCapabilities>? table)
    {
        if (table is null || table.Count == 0) return null;
        if (table.TryGetValue(model, out var exact)) return exact;

        AnthropicModelCapabilities? best = null;
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

    private static Dictionary<string, AnthropicModelCapabilities> BuildBuiltIn()
    {
        var table = new Dictionary<string, AnthropicModelCapabilities>(StringComparer.Ordinal);

        // Claude 4.x — budget-style thinking; forced tool choice supported.
        var budget = new AnthropicModelCapabilities { ThinkingStyle = AnthropicThinkingStyle.Budget };
        foreach (var legacy in AnthropicHelper.LegacyModels)
        {
            table[legacy] = budget;
        }

        // Claude 5.1 (Fable / Mythos) and Opus 5.5 — adaptive thinking; tool_choice any/tool return 400.
        // Opus 5.5 also rejects disabled thinking at every effort level, which the default already covers.
        var noForcedToolChoice = new AnthropicModelCapabilities { SupportsForcedToolChoice = false };
        table["claude-fable-5-1"] = noForcedToolChoice;
        table["claude-mythos-5-1"] = noForcedToolChoice;
        table["claude-opus-5-5"] = noForcedToolChoice;

        // Adaptive thinking-off and effort rows (vendor docs 2026-09): disabled is accepted by Sonnet 5 and
        // Opus 4.6/4.7/4.8; Fable and Opus 5.5 reject it (400) and Opus 5 is steered to low effort instead; xhigh arrived
        // with Opus 4.7, so the 4.6 generation tops out at high.
        var canDisable = new AnthropicModelCapabilities { SupportsDisabledThinking = true };
        var canDisableNoXHigh = canDisable with { SupportsXHighEffort = false };
        table["claude-sonnet-5"] = canDisable;
        table["claude-opus-4-8"] = canDisable;
        table["claude-opus-4-7"] = canDisable;
        table["claude-opus-4-6"] = canDisableNoXHigh;
        table["claude-sonnet-4-6"] = canDisableNoXHigh;

        return table;
    }
}
