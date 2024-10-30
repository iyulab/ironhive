using MessagePack;

namespace Raggle.Abstractions.Memory.Document;

public enum MemorizationStatus
{
    /// <summary>
    /// 임베딩 되지 않은 상태
    /// </summary>
    NotMemorized,

    /// <summary>
    /// 임베딩 작업이 대기중인 상태
    /// </summary>
    Queued,

    /// <summary>
    /// 임베딩 중인 상태
    /// </summary>
    Memorizing,

    /// <summary>
    /// 임베딩 된 상태
    /// </summary>
    Memorized,

    /// <summary>
    /// 임베딩이 실패한 상태
    /// </summary>
    FailedMemorization,
}

[MessagePackObject]
public class DocumentRecord
{
    /// <summary>
    /// 벡터화 상태
    /// </summary>
    [Key(0)]
    public required MemorizationStatus Status { get; set; }

    /// <summary>
    /// 문서가 저장된 컬렉션의 이름
    /// </summary>
    [Key(1)]
    public required string CollectionName { get; set; }

    /// <summary>
    /// 문서의 식별자
    /// </summary>
    [Key(2)]
    public required string DocumentId { get; set; }

    /// <summary>
    /// 파일의 이름
    /// </summary>
    [Key(3)]
    public required string FileName { get; set; }

    /// <summary>
    /// 파일의 MIME 타입
    /// </summary>
    [Key(4)]
    public required string ContentType { get; set; }

    /// <summary>
    /// 파일 크기 (byte)
    /// </summary>
    [Key(5)]
    public long? ContentLength { get; set; }

    /// <summary>
    /// 문서의 태그들
    /// </summary>
    [Key(6)]
    public string[] Tags { get; set; } = [];

    /// <summary>
    /// 문서 생성 시각
    /// </summary>
    [Key(7)]
    public required DateTime CreatedAt { get; set; }

    /// <summary>
    /// 문서 업데이트 시각
    /// </summary>
    [Key(8)]
    public DateTime? LastUpdatedAt { get; set; }
}
