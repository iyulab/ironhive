using IronHive.Abstractions.Messages;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// 특정 OpenAI 모델(세대)이 Responses API wire 에서 받아들이는 것 — 「요청된 의도」를 「그 모델이 받는 wire」로
/// 번역할 때 생성기가 참조하는 정책입니다.
/// </summary>
/// <remarks>
/// <para>
/// 모델 세대 규칙은 vendor 지식이므로 이 provider 안에 둡니다(<c>IronHive.Abstractions</c>는 provider 중립).
/// 내장 표는 <see cref="Resolve"/>가 모델 id 의 <b>가장 긴 접두 일치</b>로 찾고, 소비자는
/// <see cref="OpenAIConfig.ModelCapabilities"/>로 모델(또는 접두)별 항목을 덮어써 새 모델을 코드 수정 없이
/// 선언할 수 있습니다 — 소비자 항목이 내장 표보다 우선합니다.
/// </para>
/// <para>
/// 근거(2026-09-17 실측, <c>POST /v1/responses</c> 의 값 검증 오류 원문): gpt-4 계열은 <c>reasoning.effort</c> 자체를
/// 받지 않고, gpt-5 는 <c>minimal</c>~<c>high</c>, gpt-5.1 은 <c>none</c>~<c>high</c>, gpt-5.4 · gpt-5.5 는
/// <c>none</c>~<c>xhigh</c>, gpt-5.6 은 <c>max</c> 까지, o 계열은 <c>low</c>~<c>high</c> 만 받습니다.
/// </para>
/// </remarks>
public sealed record OpenAIModelCapabilities
{
    private static readonly string[] Ranks = ["none", "minimal", "low", "medium", "high", "xhigh", "max"];

    /// <summary>
    /// 이 모델이 받는 <c>reasoning.effort</c> 값(wire 문자열). <c>null</c>이면 추론 파라미터를 받지 않는 모델로 보고
    /// <c>reasoning</c> 을 보내지 않습니다. 기본값은 최신 세대(<c>none</c>·<c>low</c>·<c>medium</c>·<c>high</c>·<c>xhigh</c>).
    /// </summary>
    public IReadOnlyList<string>? ReasoningEfforts { get; init; } = ["none", "low", "medium", "high", "xhigh"];

    /// <summary>
    /// 내장 표에 없는 모델의 정책 — 최신 세대와 같다고 가정합니다.
    /// </summary>
    public static OpenAIModelCapabilities Default { get; } = new();

    /// <summary>
    /// 내장 표. 키는 모델 id 또는 그 접두.
    /// </summary>
    public static IReadOnlyDictionary<string, OpenAIModelCapabilities> BuiltIn { get; } = BuildBuiltIn();

    /// <summary>
    /// 모델 id 에 적용할 정책을 찾습니다. 소비자 항목(<paramref name="overrides"/>) → 내장 표 순으로, 각각
    /// 정확 일치 → 가장 긴 접두 일치로 찾고, 어디에도 없으면 <see cref="Default"/>.
    /// </summary>
    public static OpenAIModelCapabilities Resolve(
        string model,
        IReadOnlyDictionary<string, OpenAIModelCapabilities>? overrides = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(model);
        return Match(model, overrides) ?? Match(model, BuiltIn) ?? Default;
    }

    /// <summary>
    /// 요청의 노력도를 이 모델이 받는 wire 값으로 번역합니다. <c>null</c>이면 <c>reasoning</c> 을 보내지 않습니다.
    /// </summary>
    /// <remarks>
    /// 받는 값이면 그대로. <see cref="MessageThinkingEffort.None"/>은 <c>none</c>, 없으면 가장 낮은 값(끌 수 없는 모델).
    /// 그 밖의 단계는 <c>none</c>을 제외한 가장 가까운 값(동률이면 높은 쪽 — <c>Minimal</c>은 「조금은 생각한다」)입니다.
    /// </remarks>
    public string? ToWireEffort(MessageThinkingEffort? effort)
    {
        if (effort is null || ReasoningEfforts is not { Count: > 0 } supported)
            return null;

        var target = effort switch
        {
            MessageThinkingEffort.None => "none",
            MessageThinkingEffort.Minimal => "minimal",
            MessageThinkingEffort.Low => "low",
            MessageThinkingEffort.Medium => "medium",
            MessageThinkingEffort.High => "high",
            _ => "xhigh",
        };
        if (supported.Contains(target))
            return target;

        var ranked = supported.Where(v => Array.IndexOf(Ranks, v) >= 0).OrderBy(v => Array.IndexOf(Ranks, v)).ToList();
        if (ranked.Count == 0)
            return null;
        if (effort is MessageThinkingEffort.None)
            return ranked[0];

        var targetRank = Array.IndexOf(Ranks, target);
        return ranked
            .Where(v => v != "none")
            .OrderBy(v => Math.Abs(Array.IndexOf(Ranks, v) - targetRank))
            .ThenByDescending(v => Array.IndexOf(Ranks, v))
            .FirstOrDefault();
    }

    private static OpenAIModelCapabilities? Match(
        string model,
        IReadOnlyDictionary<string, OpenAIModelCapabilities>? table)
    {
        if (table is null || table.Count == 0) return null;
        if (table.TryGetValue(model, out var exact)) return exact;

        OpenAIModelCapabilities? best = null;
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

    private static Dictionary<string, OpenAIModelCapabilities> BuildBuiltIn()
    {
        var oSeries = new OpenAIModelCapabilities { ReasoningEfforts = ["low", "medium", "high"] };
        return new Dictionary<string, OpenAIModelCapabilities>(StringComparer.Ordinal)
        {
            // gpt-4o / gpt-4.1 — "Unsupported parameter: 'reasoning.effort' is not supported with this model."
            ["gpt-4"] = new() { ReasoningEfforts = null },
            // gpt-5 / gpt-5-mini / gpt-5-nano (original generation) — no none, no xhigh.
            ["gpt-5"] = new() { ReasoningEfforts = ["minimal", "low", "medium", "high"] },
            // gpt-5.x — the latest generation's set unless a narrower row below matches.
            ["gpt-5."] = Default,
            ["gpt-5.1"] = new() { ReasoningEfforts = ["none", "low", "medium", "high"] },
            ["gpt-5.6"] = new() { ReasoningEfforts = ["none", "low", "medium", "high", "xhigh", "max"] },
            ["o1"] = oSeries,
            ["o3"] = oSeries,
            ["o4"] = oSeries,
        };
    }
}
