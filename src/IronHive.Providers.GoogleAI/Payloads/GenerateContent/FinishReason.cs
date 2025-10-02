namespace IronHive.Providers.GoogleAI.Payloads.GenerateContent;

/// <summary>
/// 생성 종료 사유
/// </summary>
internal enum FinishReason
{
    FINISH_REASON_UNSPECIFIED,
    STOP, // 정상 종료
    MAX_TOKENS, // 최대 토큰 수 도달
    SAFETY, // 안전 필터링에 의한 종료
    RECITATION, // 인용문 생성
    LANGUAGE, // 언어 모델 변경
    OTHER, // 기타
    BLOCKLIST, // 차단 목록에 의한 종료
    PROHIBITED_CONTENT, // 금지된 콘텐츠
    SPII, // 민감한 개인 식별 정보
    MALFORMED_FUNCTION_CALL, // 잘못된 함수 호출
    IMAGE_SAFETY, // 이미지 안전성
    UNEXPECTED_TOOL_CALL, // 예상치 못한 도구 호출
    TOO_MANY_TOOL_CALLS // 도구 호출 과다
}