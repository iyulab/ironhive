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

/// <summary>압축기에 전달되는 컨텍스트입니다.</summary>
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
}
