namespace IronHive.Abstractions.Messages;

/// <summary>
/// 추론 토큰 예산을 응답과 함께 쓰는 출력 한도(<see cref="MessageGenerationRequest.MaxTokens"/>) 안에 맞춥니다.
/// </summary>
/// <remarks>
/// 예산 방식의 추론(OpenAI 호환 서버의 <c>reasoning_budget_tokens</c>, Anthropic <c>budget_tokens</c>, Gemini 2.5
/// <c>thinkingBudget</c>)은 추론과 답이 한 출력 한도를 나눠 씁니다. 한도보다 큰 예산은 추론이 한도를 다 쓰고 빈 답으로
/// 끝나게 하므로, 모든 provider 가 같은 규칙으로 답의 자리를 먼저 남깁니다.
/// </remarks>
public static class ThinkingBudget
{
    /// <summary>답을 위해 남기는 최소 토큰 수. 한도의 4분의 1이 이보다 크면 그쪽을 남깁니다.</summary>
    public const int MinimumAnswerTokens = 256;

    /// <summary>
    /// <paramref name="budget"/> 중 <paramref name="maxTokens"/> 안에 들어가는 부분: 한도에서 답의 자리
    /// (<see cref="MinimumAnswerTokens"/> 와 한도의 4분의 1 중 큰 쪽)를 뺀 값과 예산 중 작은 쪽입니다. 0 이면 그 한도에서는
    /// 생각할 자리가 없다는 뜻이며, provider 는 추론을 켜지 않습니다. 한도가 없으면(null) 예산을 그대로 돌려줍니다.
    /// </summary>
    /// <example>Medium(1,024) · MaxTokens 256 → 0 · MaxTokens 2,000 → 1,024 · MaxTokens 1,100 → 825.</example>
    public static int FitWithin(int budget, int? maxTokens)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(budget);
        if (maxTokens is not { } max)
            return budget;

        var answer = Math.Max(MinimumAnswerTokens, max / 4);
        return Math.Clamp(max - answer, 0, budget);
    }
}
