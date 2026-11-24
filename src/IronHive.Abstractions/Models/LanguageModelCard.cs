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
    /// (GoogleAI의 <c>inputTokenLimit</c>, Anthropic의 <c>max_input_tokens</c>, OpenAI 와이어 서버·게이트웨이의 <c>max_model_len</c>·
    /// <c>context_length</c>·<c>context_window</c> 등). OpenAI 자체의 모델 목록 API는 컨텍스트 크기를
    /// 반환하지 않으므로 null로 남으며, 이는 결함이 아니다 — 정적 카탈로그를 하드코딩해 채우는 것은
    /// 모델 메타데이터 카탈로그의 책임(TokenMeter)이지 프로바이더 어댑터의 책임이 아니다.
    /// 값이 항상 필요한 소비자는 이 필드를 그런 카탈로그로 보완해야 한다.
    /// </para>
    /// </summary>
    public int? ContextWindow { get; init; }

    /// <summary>
    /// 단일 응답에서 생성 가능한 최대 토큰 수. <see cref="ContextWindow"/>와 같이 best-effort — 모델 목록이 보고할 때만(GoogleAI
    /// <c>outputTokenLimit</c>, Anthropic <c>max_tokens</c>, OpenAI 와이어 <c>max_output_tokens</c>·<c>max_completion_tokens</c>·
    /// <c>top_provider.max_completion_tokens</c>).
    /// </summary>
    public int? MaxOutputTokens { get; init; }

    /// <summary>
    /// 모델이 제공하는 기능(예: tool-calling, structured output, web-search 등).
    /// <para>
    /// <b>best-effort — null이 정상값이다.</b> 모델 목록 API가 능력을 알려 줄 때만 채워진다(Anthropic의 <c>capabilities</c>:
    /// <c>thinking</c>, <c>effort</c>, <c>structured-output</c>, <c>citations</c>, <c>batch</c>, <c>web-search</c>,
    /// <c>code-execution</c> 중 지원한다고 보고한 것). 목록이 있는데 비어 있으면 «전부 미지원으로 보고됨», null이면 «알려 주지 않음»이다.
    /// </para>
    /// </summary>
    public IReadOnlyCollection<string>? Features { get; init; }

    /// <summary>
    /// 허용 입력 모달리티(예: "text", "image", "audio"). <see cref="Features"/>와 같이 best-effort — Anthropic은
    /// <c>text</c> + 지원한다고 보고한 <c>image</c>·<c>pdf</c>, OpenAI 와이어 게이트웨이는 목록이 말할 때만(OpenRouter의
    /// <c>architecture.input_modalities</c>, 없으면 <c>architecture.modality</c>의 입력 쪽 — 낱말은 벤더가 쓴 그대로 소문자,
    /// 예: <c>file</c>). vLLM·llama.cpp 목록은 아무것도 말하지 않으므로 null이 정상이다.
    /// </summary>
    public IReadOnlyCollection<string>? InputModalities { get; init; }

    /// <summary>
    /// 생성 가능한 출력 모달리티(예: "text", "image", "audio").
    /// </summary>
    public IReadOnlyCollection<string>? OutputModalities { get; init; }
}
