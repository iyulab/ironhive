using System.Runtime.CompilerServices;
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
    private readonly ResilienceOptions _options;

    /// <summary>
    /// ResilientMessageGenerator의 새 인스턴스를 생성합니다.
    /// </summary>
    /// <param name="inner">내부 메시지 생성기</param>
    /// <param name="options">복원력 옵션</param>
    public ResilientMessageGenerator(IMessageGenerator inner, ResilienceOptions? options = null)
        : base(inner)
    {
        _options = options ?? ResiliencePipelineFactory.DefaultOptions;
        _pipeline = ResiliencePipelineFactory.Create<MessageResponse>(_options);
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
    public override async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // 비동기용 파이프라인으로 초기 연결(첫 번째 요소 취득까지)에 resilience 적용
        var asyncPipeline = ResiliencePipelineFactory.CreateAsync(_options);

        IAsyncEnumerator<StreamingMessageResponse>? enumerator = null;
        StreamingMessageResponse? firstItem = null;

        await asyncPipeline.ExecuteAsync(async ct =>
        {
            enumerator = InnerGenerator
                .GenerateStreamingMessageAsync(request, ct)
                .GetAsyncEnumerator(ct);
            if (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                firstItem = enumerator.Current;
            }
        }, cancellationToken).ConfigureAwait(false);

        if (firstItem is not null)
        {
            yield return firstItem;
        }

        if (enumerator is not null)
        {
            try
            {
                while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    yield return enumerator.Current;
                }
            }
            finally
            {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
