namespace IronHive.Abstractions.Messages;

/// <summary>
/// 응답에 모델의 추론(thinking) 내용을 얼마나 싣도록 요청할지 정의합니다.
/// 추론을 할지·얼마나 할지는 <see cref="MessageThinkingEffort"/>가 정하고, 이 값은 그 결과를 보여 줄지만 정합니다.
/// </summary>
public enum MessageThinkingOutput
{
    /// <summary>
    /// 추론 내용을 응답에 싣지 않습니다. 모델은 여전히 추론할 수 있고, 멀티턴 연속성에 필요한 불투명 서명은 유지됩니다.
    /// </summary>
    None,

    /// <summary>
    /// 추론 요약을 싣습니다.
    /// </summary>
    Summary,

    /// <summary>
    /// provider 가 제공하는 가장 자세한 추론 내용을 싣습니다. 원문 사고 과정을 주지 않는 provider 는 가장 자세한 요약을 줍니다.
    /// </summary>
    Full,
}
