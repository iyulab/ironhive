using System.Runtime.CompilerServices;
using System.Threading.Channels;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Streaming;

namespace IronHive.Core.Streaming;

/// <summary>
/// 스트리밍 재개를 지원하는 메시지 생성기 래퍼입니다.
/// </summary>
public class ResumableStreamingGenerator
{
    private readonly IMessageGenerator _generator;
    private readonly IStreamStateManager _stateManager;
    private readonly StreamStateOptions _options;

    public ResumableStreamingGenerator(
        IMessageGenerator generator,
        IStreamStateManager stateManager,
        StreamStateOptions? options = null)
    {
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
        _options = options ?? new StreamStateOptions();
    }

    /// <summary>
    /// 새 스트리밍 요청을 시작합니다.
    /// </summary>
    /// <param name="request">메시지 생성 요청</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>스트림 ID와 스트리밍 응답</returns>
    public async IAsyncEnumerable<ResumableStreamChunk> StartStreamingAsync(
        MessageGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var state = await _stateManager.CreateStateAsync(
            metadata: new Dictionary<string, object>
            {
                ["model"] = request.Model,
                ["startedAt"] = DateTime.UtcNow
            },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var streamId = state.StreamId;
        var channel = Channel.CreateUnbounded<ResumableStreamChunk>();

        // 백그라운드에서 스트리밍 처리
        _ = ProcessStreamingAsync(streamId, request, channel.Writer, cancellationToken);

        // 채널에서 결과 읽기
        await foreach (var chunk in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            yield return chunk;
        }
    }

    private async Task ProcessStreamingAsync(
        string streamId,
        MessageGenerationRequest request,
        ChannelWriter<ResumableStreamChunk> writer,
        CancellationToken cancellationToken)
    {
        var chunkIndex = 0;

        try
        {
            await foreach (var chunk in _generator.GenerateStreamingMessageAsync(request, cancellationToken)
                .ConfigureAwait(false))
            {
                // 청크 저장 (TextContent인 경우만)
                if (chunk is StreamingContentDeltaResponse deltaResponse)
                {
                    if (deltaResponse.Delta is TextDeltaContent textDelta)
                    {
                        await _stateManager.AppendChunkAsync(
                            streamId,
                            textDelta.Value ?? "",
                            chunkIndex,
                            cancellationToken).ConfigureAwait(false);
                    }
                }

                await writer.WriteAsync(
                    new ResumableStreamChunk(streamId, chunk, chunkIndex),
                    cancellationToken).ConfigureAwait(false);

                chunkIndex++;
            }

            // 성공적으로 완료
            await _stateManager.UpdateStatusAsync(
                streamId,
                StreamStatus.Completed,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // 취소됨 - 재개 가능 상태로 표시
            await _stateManager.MarkAsDisconnectedAsync(streamId, CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // 오류 발생 - 재개 가능 상태로 표시
            await _stateManager.UpdateStatusAsync(
                streamId,
                StreamStatus.Disconnected,
                ex.Message,
                CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            writer.Complete();
        }
    }

    /// <summary>
    /// 이전에 중단된 스트림을 재개합니다.
    /// </summary>
    /// <param name="streamId">재개할 스트림 ID</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>재개 결과</returns>
    public async Task<StreamResumeResult> ResumeStreamAsync(
        string streamId,
        CancellationToken cancellationToken = default)
    {
        var state = await _stateManager.GetStateAsync(streamId, cancellationToken)
            .ConfigureAwait(false);

        if (state == null)
        {
            return StreamResumeResult.NotFound(streamId);
        }

        if (!state.CanResume)
        {
            return StreamResumeResult.CannotResume(
                streamId,
                state.Status,
                $"Stream status '{state.Status}' does not allow resumption.");
        }

        // 재개 윈도우 확인
        var timeSinceDisconnect = DateTime.UtcNow - state.LastUpdatedAt;
        if (timeSinceDisconnect > _options.ResumeWindow)
        {
            await _stateManager.UpdateStatusAsync(
                streamId,
                StreamStatus.Expired,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return StreamResumeResult.Expired(streamId, timeSinceDisconnect);
        }

        return StreamResumeResult.Success(
            streamId,
            state.AccumulatedContent,
            state.LastChunkIndex,
            state.TotalChunksReceived);
    }

    /// <summary>
    /// 재개 가능한 스트림 목록을 조회합니다.
    /// </summary>
    public async Task<IReadOnlyList<IStreamState>> GetResumableStreamsAsync(
        CancellationToken cancellationToken = default)
    {
        var streamIds = await _stateManager.GetResumableStreamsAsync(cancellationToken)
            .ConfigureAwait(false);

        var states = new List<IStreamState>();
        foreach (var id in streamIds)
        {
            var state = await _stateManager.GetStateAsync(id, cancellationToken)
                .ConfigureAwait(false);
            if (state != null)
            {
                states.Add(state);
            }
        }

        return states;
    }
}

/// <summary>
/// 재개 가능한 스트림 청크
/// </summary>
public sealed record ResumableStreamChunk(
    string StreamId,
    StreamingMessageResponse Response,
    int ChunkIndex);

/// <summary>
/// 스트림 재개 결과
/// </summary>
public sealed class StreamResumeResult
{
    public bool IsSuccess { get; init; }
    public string StreamId { get; init; } = string.Empty;
    public StreamResumeFailureReason? FailureReason { get; init; }
    public string? ErrorMessage { get; init; }

    // 성공 시 데이터
    public string? AccumulatedContent { get; init; }
    public int LastChunkIndex { get; init; }
    public int TotalChunksReceived { get; init; }

    public static StreamResumeResult Success(
        string streamId,
        string accumulatedContent,
        int lastChunkIndex,
        int totalChunksReceived) => new()
    {
        IsSuccess = true,
        StreamId = streamId,
        AccumulatedContent = accumulatedContent,
        LastChunkIndex = lastChunkIndex,
        TotalChunksReceived = totalChunksReceived
    };

    public static StreamResumeResult NotFound(string streamId) => new()
    {
        IsSuccess = false,
        StreamId = streamId,
        FailureReason = StreamResumeFailureReason.NotFound,
        ErrorMessage = $"Stream '{streamId}' not found."
    };

    public static StreamResumeResult CannotResume(
        string streamId,
        StreamStatus status,
        string message) => new()
    {
        IsSuccess = false,
        StreamId = streamId,
        FailureReason = StreamResumeFailureReason.InvalidState,
        ErrorMessage = message
    };

    public static StreamResumeResult Expired(string streamId, TimeSpan elapsed) => new()
    {
        IsSuccess = false,
        StreamId = streamId,
        FailureReason = StreamResumeFailureReason.Expired,
        ErrorMessage = $"Stream '{streamId}' expired after {elapsed.TotalMinutes:F1} minutes."
    };
}

/// <summary>
/// 스트림 재개 실패 사유
/// </summary>
public enum StreamResumeFailureReason
{
    /// <summary>
    /// 스트림을 찾을 수 없음
    /// </summary>
    NotFound,

    /// <summary>
    /// 재개할 수 없는 상태
    /// </summary>
    InvalidState,

    /// <summary>
    /// 재개 윈도우 만료
    /// </summary>
    Expired,

    /// <summary>
    /// 최대 재개 시도 횟수 초과
    /// </summary>
    MaxAttemptsExceeded
}
