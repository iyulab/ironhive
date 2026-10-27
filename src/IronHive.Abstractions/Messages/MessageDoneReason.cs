namespace IronHive.Abstractions.Messages;

/// <summary>
/// LLM 작업에서 채팅 응답이 중단된 사유를 나타내는 열거형입니다.
/// </summary>
public enum MessageDoneReason
{
    /// <summary>
    /// 어시스턴트의 응답 턴이 완료됨.
    /// </summary>
    EndTurn,

    /// <summary>
    /// 모델이 툴 실행을 위한 도구 호출을 요구함
    /// </summary>
    ToolCall,

    /// <summary>
    /// 출력 토큰의 최대 수에 도달함.
    /// </summary>
    MaxTokens,

    /// <summary>
    /// 필터링된(제한된) 텍스트 콘텐츠가 탐지됨.
    /// </summary>
    ContentFilter,

    /// <summary>
    /// 중단(stop) 시퀀스 문자열을 만남.
    /// </summary>
    StopSequence,

    /// <summary>
    /// 이외 알 수 없는 이유.
    /// </summary>
    Unknown,

    /// <summary>
    /// 응답 턴이 끝나지 않고 멈춤 — 공급자가 긴 작업(서버 도구)을 중간에 멈췄고, 응답을 그대로 다시 보내야 이어집니다
    /// (Anthropic <c>pause_turn</c>). 메시지 루프는 이어 가지 않고 이 사유로 턴을 끝내며, 이어 갈지는 호출자가 정합니다.
    /// </summary>
    Paused = 6,

    /// <summary>
    /// 컨텍스트 창이 가득 차서 멈춤 (Anthropic <c>model_context_window_exceeded</c>). <see cref="MaxTokens"/> 는 출력
    /// 한도를 올리면 풀리지만 이쪽은 컨텍스트를 줄여야 풀립니다.
    /// </summary>
    ContextExceeded = 7
}
