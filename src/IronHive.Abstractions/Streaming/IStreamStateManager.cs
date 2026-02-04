namespace IronHive.Abstractions.Streaming;

/// <summary>
/// 스트림 상태를 관리하는 인터페이스입니다.
/// </summary>
public interface IStreamStateManager
{
    /// <summary>
    /// 새 스트림 상태를 생성합니다.
    /// </summary>
    /// <param name="streamId">스트림 ID (null이면 자동 생성)</param>
    /// <param name="metadata">초기 메타데이터</param>
    /// <returns>생성된 스트림 상태</returns>
    Task<IStreamState> CreateStateAsync(
        string? streamId = null,
        IDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 스트림 상태를 조회합니다.
    /// </summary>
    /// <param name="streamId">스트림 ID</param>
    /// <returns>스트림 상태 (없으면 null)</returns>
    Task<IStreamState?> GetStateAsync(
        string streamId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 스트림에 청크를 추가합니다.
    /// </summary>
    /// <param name="streamId">스트림 ID</param>
    /// <param name="chunkContent">청크 콘텐츠</param>
    /// <param name="chunkIndex">청크 인덱스</param>
    Task AppendChunkAsync(
        string streamId,
        string chunkContent,
        int chunkIndex,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 스트림 상태를 업데이트합니다.
    /// </summary>
    /// <param name="streamId">스트림 ID</param>
    /// <param name="status">새 상태</param>
    /// <param name="errorMessage">오류 메시지 (선택)</param>
    Task UpdateStatusAsync(
        string streamId,
        StreamStatus status,
        string? errorMessage = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 스트림을 재개 가능한 상태로 표시합니다.
    /// </summary>
    /// <param name="streamId">스트림 ID</param>
    Task MarkAsDisconnectedAsync(
        string streamId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 스트림 상태를 삭제합니다.
    /// </summary>
    /// <param name="streamId">스트림 ID</param>
    Task DeleteStateAsync(
        string streamId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 만료된 스트림 상태를 정리합니다.
    /// </summary>
    /// <param name="expirationTime">만료 기준 시간</param>
    /// <returns>삭제된 스트림 수</returns>
    Task<int> CleanupExpiredAsync(
        TimeSpan expirationTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 재개 가능한 스트림 목록을 조회합니다.
    /// </summary>
    /// <returns>재개 가능한 스트림 ID 목록</returns>
    Task<IReadOnlyList<string>> GetResumableStreamsAsync(
        CancellationToken cancellationToken = default);
}
