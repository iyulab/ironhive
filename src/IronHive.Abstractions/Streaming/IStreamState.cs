namespace IronHive.Abstractions.Streaming;

/// <summary>
/// 스트리밍 상태를 나타내는 인터페이스입니다.
/// 연결 끊김 후 스트림 재개를 지원합니다.
/// </summary>
public interface IStreamState
{
    /// <summary>
    /// 스트림 고유 식별자
    /// </summary>
    string StreamId { get; }

    /// <summary>
    /// 스트림 생성 시간
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// 마지막 업데이트 시간
    /// </summary>
    DateTime LastUpdatedAt { get; }

    /// <summary>
    /// 현재 스트림 상태
    /// </summary>
    StreamStatus Status { get; }

    /// <summary>
    /// 마지막으로 처리된 청크 인덱스
    /// </summary>
    int LastChunkIndex { get; }

    /// <summary>
    /// 총 수신된 청크 수
    /// </summary>
    int TotalChunksReceived { get; }

    /// <summary>
    /// 누적된 콘텐츠
    /// </summary>
    string AccumulatedContent { get; }

    /// <summary>
    /// 스트림이 재개 가능한지 여부
    /// </summary>
    bool CanResume { get; }

    /// <summary>
    /// 오류 메시지 (실패 시)
    /// </summary>
    string? ErrorMessage { get; }

    /// <summary>
    /// 추가 메타데이터
    /// </summary>
    IReadOnlyDictionary<string, object>? Metadata { get; }
}

/// <summary>
/// 스트림 상태
/// </summary>
public enum StreamStatus
{
    /// <summary>
    /// 스트림 시작 대기 중
    /// </summary>
    Pending,

    /// <summary>
    /// 스트리밍 진행 중
    /// </summary>
    Streaming,

    /// <summary>
    /// 일시 중단됨 (재개 가능)
    /// </summary>
    Paused,

    /// <summary>
    /// 연결 끊김 (재개 가능)
    /// </summary>
    Disconnected,

    /// <summary>
    /// 성공적으로 완료됨
    /// </summary>
    Completed,

    /// <summary>
    /// 오류로 실패함
    /// </summary>
    Failed,

    /// <summary>
    /// 취소됨
    /// </summary>
    Cancelled,

    /// <summary>
    /// 만료됨 (재개 불가)
    /// </summary>
    Expired
}
