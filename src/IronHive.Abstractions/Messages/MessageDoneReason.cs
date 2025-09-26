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
    Unknown
}
