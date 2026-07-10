namespace IronHive.Abstractions.Messages;

/// <summary>
/// 컨텍스트 예산 초과 시 대화 메시지를 예산 내로 줄이는 압축기입니다.
/// 구현은 소비자 소유입니다 (예: 요약 후 세션 저장소에 영속화하는 앱 측 압축기).
/// 기본 구현은 제공되지 않습니다 — 압축 전략(요약 프롬프트, 보존 범위)은 앱 도메인 결정입니다.
/// </summary>
public interface IMessageCompactor
{
    /// <summary>
    /// <paramref name="context"/>의 메시지를 예산 내로 압축한 새 메시지 컬렉션을 반환합니다.
    /// 도구 호출/결과 페어 등 프로바이더 요청 유효성 불변식은 구현이 보존해야 합니다.
    /// </summary>
    Task<IList<Message>> CompactAsync(MessageCompactionContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// 압축기에 전달되는 컨텍스트입니다.
/// <para>
/// 파이프라인 불변식 (append-only): 파이프라인은 기준 목록(요청 시작 시점의 원본 메시지,
/// 또는 직전 압축이 반환한 메시지)의 <b>뒤에만</b> 메시지를 추가하며, 기존 메시지를
/// 재배열·복제·변형하지 않습니다. <see cref="OriginalMessageCount"/>와
/// <see cref="PreviousCompactedMessages"/>의 경계 의미는 이 불변식에 기초합니다.
/// </para>
/// <para>
/// 같은 요청 안에서 압축은 여러 번 발동할 수 있으며(툴 루프 매 반복 재검사),
/// 호출은 항상 순차적입니다. 단 <see cref="ContextPolicy"/>(및 그 Compactor)를
/// 여러 요청에서 공유하면 동시 호출될 수 있으므로, 요청별 상태는 인스턴스 필드가 아니라
/// 본 컨텍스트를 통해 판단해야 합니다.
/// </para>
/// </summary>
public class MessageCompactionContext
{
    /// <summary>압축 대상 메시지 (원본 순서).</summary>
    public required IReadOnlyList<Message> Messages { get; init; }

    /// <summary>초과가 감지된 시점의 추정 입력 토큰 수.</summary>
    public required int EstimatedTokens { get; init; }

    /// <summary>입력 토큰 예산 (<see cref="ContextPolicy.MaxInputTokens"/>).</summary>
    public required int BudgetTokens { get; init; }

    /// <summary>요청 모델 식별자.</summary>
    public required string Model { get; init; }

    /// <summary>요청의 시스템 프롬프트 (압축 대상 아님 — 참고용).</summary>
    public string? System { get; init; }

    /// <summary>
    /// 요청 시작 시점(<c>MessageRequest.Messages</c>)에 존재했던 메시지 수.
    /// 첫 압축 시 <see cref="Messages"/>의 앞에서부터 이 개수까지가 소비자가 구성한 원본이며,
    /// 그 이후는 파이프라인이 툴 루프에서 추가한 메시지입니다.
    /// (예: 영속형 압축기가 "저장소 기준 몇 번째까지 요약했는가"를 산출할 때 사용)
    /// </summary>
    public required int OriginalMessageCount { get; init; }

    /// <summary>
    /// 동일 요청 내 직전 <see cref="IMessageCompactor.CompactAsync"/>가 반환한 메시지 컬렉션.
    /// 첫 압축이면 null입니다. null이 아니면 <see cref="Messages"/>의 앞에서부터 이 컬렉션
    /// 길이까지가 직전 압축 결과이며, 그 이후는 직전 압축 이후 파이프라인이 추가한 메시지입니다.
    /// </summary>
    public IReadOnlyList<Message>? PreviousCompactedMessages { get; init; }
}
