namespace IronHive.Abstractions.Messages;

/// <summary>
/// LLM(대규모 언어 모델) 사용 시 토큰 사용량을 나타내는 클래스입니다.
/// </summary>
public class MessageTokenUsage
{
    /// <summary>
    /// 입력에 사용된 토큰 수입니다. 캐시에서 읽은 입력(<see cref="CachedInputTokens"/>)과 캐시에 새로 쓴 입력을 포함한
    /// 전체 입력입니다 — 공급자가 캐시분을 따로 보고해도(Anthropic 의 <c>input_tokens</c> 는 캐시분을 제외한다) 합쳐서 싣습니다.
    /// </summary>
    public int InputTokens { get; set; }

    /// <summary>
    /// <see cref="InputTokens"/> 중 공급자의 프롬프트 캐시에서 읽은 토큰 수입니다(OpenAI <c>cached_tokens</c>,
    /// Anthropic <c>cache_read_input_tokens</c>, Gemini <c>cachedContentTokenCount</c>). 공급자가 보고하지 않으면 <c>null</c> —
    /// 0 과 구분됩니다(0 은 «보고했고 캐시 적중 없음»).
    /// </summary>
    public int? CachedInputTokens { get; set; }

    /// <summary>
    /// 출력에 사용된 토큰 수입니다.
    /// </summary>
    public int OutputTokens { get; set; }

    /// <summary>
    /// 총 사용된 토큰 수입니다. (입력 + 출력)
    /// </summary>
    public int TotalTokens => InputTokens + OutputTokens;

    /// <summary>
    /// 두 사용량의 합입니다 — 도구 루프처럼 한 호출이 여러 요청으로 이뤄질 때 호출 전체의 사용량입니다. 한쪽이 null 이면
    /// 다른 쪽을, 둘 다 null 이면 null 을 돌려줍니다. <see cref="CachedInputTokens"/> 는 어느 한쪽이라도 보고했으면 합산하고
    /// 둘 다 보고하지 않았으면 null 로 둡니다.
    /// </summary>
    public static MessageTokenUsage? Add(MessageTokenUsage? left, MessageTokenUsage? right)
    {
        if (left is null) return right;
        if (right is null) return left;
        return new MessageTokenUsage
        {
            InputTokens = left.InputTokens + right.InputTokens,
            OutputTokens = left.OutputTokens + right.OutputTokens,
            CachedInputTokens = left.CachedInputTokens is null && right.CachedInputTokens is null
                ? null
                : (left.CachedInputTokens ?? 0) + (right.CachedInputTokens ?? 0),
        };
    }
}