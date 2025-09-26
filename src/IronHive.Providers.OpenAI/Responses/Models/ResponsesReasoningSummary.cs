namespace IronHive.Providers.OpenAI.Responses.Models;

internal enum ResponsesReasoningSummary
{
    /// <summary>
    /// 자동 설정 (기본값)
    /// </summary>
    Auto,

    /// <summary>
    /// computer_use 모델에서만 지원됨
    /// </summary>
    Concise,

    /// <summary>
    /// 자세한 설명
    /// </summary>
    Detailed
}
