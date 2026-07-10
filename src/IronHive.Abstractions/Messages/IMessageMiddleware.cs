namespace IronHive.Abstractions.Messages;

/// <summary>
/// 메시지 생성(generator 호출) 전후를 인터셉션하는 미들웨어입니다.
/// MessageService의 루프 반복마다 한 번씩 호출됩니다.
/// </summary>
public interface IMessageMiddleware
{
    /// <summary>
    /// 메시지 생성을 인터셉트합니다.
    /// next를 호출하지 않으면 short-circuit됩니다.
    /// 기본 구현은 아무 처리 없이 next로 통과시킵니다.
    /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords — 'next' is the standard middleware pipeline convention
    Task<MessageResponse> GenerateAsync(
        MessageGenerationRequest request,
        Func<MessageGenerationRequest, Task<MessageResponse>> next,
        CancellationToken cancellationToken = default)
        => next(request);

    /// <summary>
    /// 스트리밍 메시지 생성을 인터셉트합니다.
    /// 기본 구현은 아무 처리 없이 next로 통과시킵니다.
    /// </summary>
    IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingAsync(
        MessageGenerationRequest request,
        Func<MessageGenerationRequest, IAsyncEnumerable<StreamingMessageResponse>> next,
        CancellationToken cancellationToken = default)
        => next(request);
#pragma warning restore CA1716
}
