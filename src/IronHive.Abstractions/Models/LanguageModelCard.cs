namespace IronHive.Abstractions.Models;

/// <summary>
/// 언어 모델(LLM) 전용 카드입니다.
/// </summary>
public sealed record LanguageModelCard : ModelCard
{
    /// <summary>
    /// 문맥 창 크기(토큰). 한 번의 대화 턴에서 모델이 고려할 수 있는 총 토큰 수(입력+출력).
    /// <para>
    /// <b>best-effort — null이 정상값이다.</b> 프로바이더의 모델 목록 API가 이 값을 노출할 때만 채워진다
    /// (현재 GoogleAI의 <c>inputTokenLimit</c>). OpenAI·Anthropic의 모델 목록 API는 컨텍스트 크기를
    /// 반환하지 않으므로 null로 남으며, 이는 결함이 아니다 — 정적 카탈로그를 하드코딩해 채우는 것은
    /// 모델 메타데이터 카탈로그의 책임(TokenMeter)이지 프로바이더 어댑터의 책임이 아니다.
    /// 값이 항상 필요한 소비자는 이 필드를 그런 카탈로그로 보완해야 한다.
    /// </para>
    /// </summary>
    public int? ContextWindow { get; init; }

    /// <summary>
    /// 단일 응답에서 생성 가능한 최대 토큰 수.
    /// </summary>
    public int? MaxOutputTokens { get; init; }

    /// <summary>
    /// 모델이 제공하는 기능(예: tool-calling, structured output, web-search 등).
    /// </summary>
    public IReadOnlyCollection<string>? Features { get; init; }

    /// <summary>
    /// 허용 입력 모달리티(예: "text", "image", "audio").
    /// </summary>
    public IReadOnlyCollection<string>? InputModalities { get; init; }

    /// <summary>
    /// 생성 가능한 출력 모달리티(예: "text", "image", "audio").
    /// </summary>
    public IReadOnlyCollection<string>? OutputModalities { get; init; }
}
