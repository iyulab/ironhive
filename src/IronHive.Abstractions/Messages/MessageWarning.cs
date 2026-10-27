namespace IronHive.Abstractions.Messages;

/// <summary>
/// 요청과 다르게 처리한 것을 응답이 알리는 항목입니다. 호출자가 요청한 설정을 공급자가 적용하지 못하고 계속 진행한 경우
/// (예외로 끝내기엔 약한 선택 사항) 응답의 <see cref="MessageResponse.Warnings"/> 에 실립니다.
/// </summary>
/// <param name="Code">변하지 않는 식별자 (<see cref="MessageWarningCodes"/>).</param>
/// <param name="Message">사람이 읽는 설명 — 무엇을 요청했고 왜 적용하지 못했는지.</param>
public sealed record MessageWarning(string Code, string Message)
{
    /// <summary>
    /// 사고(thinking)를 요청했지만 <see cref="MessageGenerationRequest.MaxTokens"/> 안에 공급자의 최소 사고 예산이
    /// 들어가지 않아 사고를 켜지 않았음을 알립니다. MaxTokens 를 자동으로 올리지 않습니다(비용이 달라지므로).
    /// </summary>
    /// <param name="minimumBudget">공급자가 받는 최소 사고 예산(토큰).</param>
    /// <param name="maxTokens">요청의 MaxTokens.</param>
    public static MessageWarning ThinkingNotApplied(int minimumBudget, int? maxTokens) => new(
        MessageWarningCodes.ThinkingNotApplied,
        $"ThinkingEffort was requested but thinking was not applied: MaxTokens ({(maxTokens is { } max ? max.ToString(System.Globalization.CultureInfo.InvariantCulture) : "unset")}) leaves less than the " +
        $"provider's minimum thinking budget of {minimumBudget} tokens once room for the answer is kept. Raise MaxTokens to get thinking.");
}

/// <summary><see cref="MessageWarning.Code"/> 값.</summary>
public static class MessageWarningCodes
{
    /// <summary>요청한 사고 설정을 적용하지 못했습니다 — <see cref="MessageWarning.ThinkingNotApplied"/>.</summary>
    public const string ThinkingNotApplied = "thinking_not_applied";
}
