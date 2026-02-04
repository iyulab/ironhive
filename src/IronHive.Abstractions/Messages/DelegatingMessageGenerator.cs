namespace IronHive.Abstractions.Messages;

/// <summary>
/// <see cref="IMessageGenerator"/>의 데코레이터 기반 확장을 위한 추상 기반 클래스입니다.
/// 내부 생성기에 대한 위임을 기본 구현으로 제공하며,
/// 파생 클래스에서 필요한 메서드만 오버라이드할 수 있습니다.
/// </summary>
public abstract class DelegatingMessageGenerator : IMessageGenerator
{
    /// <summary>
    /// 위임 대상 내부 메시지 생성기
    /// </summary>
    protected IMessageGenerator InnerGenerator { get; }

    /// <summary>
    /// DelegatingMessageGenerator의 새 인스턴스를 생성합니다.
    /// </summary>
    /// <param name="innerGenerator">위임할 내부 메시지 생성기</param>
    protected DelegatingMessageGenerator(IMessageGenerator innerGenerator)
    {
        InnerGenerator = innerGenerator ?? throw new ArgumentNullException(nameof(innerGenerator));
    }

    /// <inheritdoc />
    public virtual Task<MessageResponse> GenerateMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
        => InnerGenerator.GenerateMessageAsync(request, cancellationToken);

    /// <inheritdoc />
    public virtual IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
        => InnerGenerator.GenerateStreamingMessageAsync(request, cancellationToken);

    /// <inheritdoc />
    public virtual void Dispose()
    {
        InnerGenerator.Dispose();
        GC.SuppressFinalize(this);
    }
}
