namespace IronHive.Abstractions.Messages;

/// <summary>
/// 요청 입력이 모델 컨텍스트 윈도우를 넘기 전에 파이프라인이 선제적으로 감지·대응하도록 하는
/// 옵트인 정책입니다. 미지정 시 기존 동작(프로바이더 오류 전파)과 동일합니다.
/// 토큰 추정은 프로바이더의 <see cref="IMessageGenerator.CountTokensAsync"/>를 1차 경로로 사용하고,
/// 미지원 프로바이더(예: OpenAI 호환 서버)는 <see cref="FallbackEstimator"/>로 보완합니다.
/// </summary>
public class ContextPolicy
{
    /// <summary>
    /// 입력 토큰 예산(상한). 모델 컨텍스트 윈도우에서 출력 예약분을 뺀 값을 소비자가 지정합니다.
    /// (모델 메타데이터 조회는 앱 레이어 책임 — 예: TokenMeter)
    /// </summary>
    public required int MaxInputTokens { get; init; }

    /// <summary>예산 초과 시 동작. 기본값은 <see cref="ContextOverflowBehavior.Fail"/>입니다.</summary>
    public ContextOverflowBehavior OnOverflow { get; init; } = ContextOverflowBehavior.Fail;

    /// <summary>
    /// <see cref="ContextOverflowBehavior.Compact"/>일 때 호출되는 압축기. 미지정 시 Fail로 폴백합니다.
    /// 같은 요청 안에서 여러 번 발동할 수 있으며(툴 루프 재검사), 요청별 상태는
    /// <see cref="MessageCompactionContext"/>(OriginalMessageCount/PreviousCompactedMessages)로
    /// 전달됩니다 — 압축기 인스턴스 필드에 요청 상태를 두지 마십시오.
    /// </summary>
    public IMessageCompactor? Compactor { get; init; }

    /// <summary>
    /// 프로바이더가 토큰 카운팅을 지원하지 않을 때(<see cref="NotSupportedException"/>) 사용할
    /// 로컬 추정기. 정책이 활성인데 두 경로 모두 없으면 설정 오류로 예외가 발생합니다(무단 미집행 방지).
    /// </summary>
    public Func<MessageGenerationRequest, int>? FallbackEstimator { get; init; }
}

/// <summary>컨텍스트 예산 초과 시 파이프라인 동작입니다.</summary>
public enum ContextOverflowBehavior
{
    /// <summary>송신 전에 <c>ContextWindowExceededException</c>을 던집니다 (네트워크 미발생).</summary>
    Fail,

    /// <summary>
    /// <see cref="ContextPolicy.Compactor"/>에 위임해 메시지를 압축한 뒤 재검사합니다.
    /// 압축기가 없거나 압축 후에도 초과하면 Fail과 동일하게 동작합니다.
    /// </summary>
    Compact,
}
