using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Resilience;
using Polly;

namespace IronHive.Core.Resilience;

/// <summary>
/// Resilience Pipeline이 적용된 메시지 생성기 래퍼입니다.
/// </summary>
public class ResilientMessageGenerator : DelegatingMessageGenerator
{
    private readonly ResiliencePipeline<MessageResponse> _pipeline;

    /// <summary>
    /// ResilientMessageGenerator의 새 인스턴스를 생성합니다.
    /// </summary>
    /// <param name="inner">내부 메시지 생성기</param>
    /// <param name="options">복원력 옵션</param>
    public ResilientMessageGenerator(IMessageGenerator inner, ResilienceOptions? options = null)
        : base(inner)
    {
        var resolvedOptions = options ?? ResiliencePipelineFactory.DefaultOptions;
        _pipeline = ResiliencePipelineFactory.Create<MessageResponse>(resolvedOptions);
    }

    /// <inheritdoc />
    public override async Task<MessageResponse> GenerateMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _pipeline.ExecuteAsync(
            async ct => await InnerGenerator.GenerateMessageAsync(request, ct).ConfigureAwait(false),
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        // 스트리밍의 경우 전체 스트림에 대한 재시도는 복잡하므로
        // 초기 연결에 대해서만 resilience를 적용
        // 실제 구현에서는 스트림 청크 타임아웃 등을 별도로 처리해야 함
        return InnerGenerator.GenerateStreamingMessageAsync(request, cancellationToken);
    }
}
