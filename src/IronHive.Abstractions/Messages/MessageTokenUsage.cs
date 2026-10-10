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
    /// <see cref="InputTokens"/> 중 이번 요청이 공급자의 프롬프트 캐시에 새로 쓴 토큰 수입니다(Anthropic
    /// <c>cache_creation_input_tokens</c>, OpenAI <c>input_tokens_details.cache_write_tokens</c>). 쓰기는 보통 일반 입력보다 비싸게
    /// 매겨지므로, 비용을 다시 계산하는 소비자에게 필요한 값입니다. 공급자가 보고하지 않으면 <c>null</c>.
    /// </summary>
    public int? CacheWriteInputTokens { get; set; }

    /// <summary>
    /// <see cref="CacheWriteInputTokens"/>를 캐시 유지 시간(TTL)별로 나눈 것입니다 — 공급자가 TTL마다 다른 단가를 매기고 그 내역을
    /// 보고할 때만(Anthropic <c>cache_creation.ephemeral_5m_input_tokens</c>·<c>ephemeral_1h_input_tokens</c>). 0 토큰인 TTL은
    /// 싣지 않습니다. 보고하지 않으면 <c>null</c>.
    /// </summary>
    public IReadOnlyList<CacheWriteTokens>? CacheWritesByTtl { get; set; }

    /// <summary>
    /// <see cref="OutputTokens"/> 중 모델의 추론(생각)에 쓰인 토큰 수입니다(OpenAI <c>reasoning_tokens</c>, Gemini
    /// <c>thoughtsTokenCount</c>). 공급자가 따로 보고하지 않으면 <c>null</c>.
    /// </summary>
    public int? ReasoningTokens { get; set; }

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
    /// 다른 쪽을, 둘 다 null 이면 null 을 돌려줍니다. <see cref="CachedInputTokens"/>·<see cref="CacheWriteInputTokens"/>·
    /// <see cref="ReasoningTokens"/> 는 어느 한쪽이라도 보고했으면 합산하고 둘 다 보고하지 않았으면 null 로 둡니다.
    /// <see cref="CacheWritesByTtl"/> 는 TTL 마다 합칩니다.
    /// </summary>
    public static MessageTokenUsage? Add(MessageTokenUsage? left, MessageTokenUsage? right)
    {
        if (left is null) return right;
        if (right is null) return left;
        return new MessageTokenUsage
        {
            InputTokens = left.InputTokens + right.InputTokens,
            OutputTokens = left.OutputTokens + right.OutputTokens,
            CachedInputTokens = Sum(left.CachedInputTokens, right.CachedInputTokens),
            CacheWriteInputTokens = Sum(left.CacheWriteInputTokens, right.CacheWriteInputTokens),
            CacheWritesByTtl = left.CacheWritesByTtl is null && right.CacheWritesByTtl is null
                ? null
                : (left.CacheWritesByTtl ?? []).Concat(right.CacheWritesByTtl ?? [])
                    .GroupBy(w => w.Ttl)
                    .Select(g => new CacheWriteTokens(g.Key, g.Sum(w => w.Tokens)))
                    .OrderBy(w => w.Ttl)
                    .ToList(),
            ReasoningTokens = Sum(left.ReasoningTokens, right.ReasoningTokens),
        };
    }

    private static int? Sum(int? left, int? right)
        => left is null && right is null ? null : (left ?? 0) + (right ?? 0);
}

/// <summary>
/// 한 캐시 유지 시간(<paramref name="Ttl"/>)으로 프롬프트 캐시에 쓴 입력 토큰 수입니다 — <see cref="MessageTokenUsage.CacheWritesByTtl"/>의 한 줄.
/// </summary>
public sealed record CacheWriteTokens(TimeSpan Ttl, int Tokens);
