using MessagePack;

namespace Raggle.Abstractions.Memory;

public enum EmbeddingStatus
{
    /// <summary>
    /// 임베딩 되지 않은 상태
    /// </summary>
    NotEmbedded,

    /// <summary>
    /// 임베딩 작업이 대기 중인 상태
    /// </summary>
    Pending,

    /// <summary>
    /// 임베딩 중인 상태
    /// </summary>
    Embedding,

    /// <summary>
    /// 임베딩 된 상태
    /// </summary>
    Embedded,
}

[MessagePackObject]
public class DocumentRecord
{
    /// <summary>
    /// 벡터화 상태
    /// </summary>
    [Key(0)]
    public required EmbeddingStatus EmbeddingStatus { get; set; } = EmbeddingStatus.NotEmbedded;

    /// <summary>
    /// 문서의 식별자
    /// </summary>
    [Key(1)]
    public required string DocumentId { get; set; }

    /// <summary>
    /// 파일의 이름
    /// </summary>
    [Key(2)]
    public required string FileName { get; set; }

    /// <summary>
    /// 파일의 MIME 타입
    /// </summary>
    [Key(3)]
    public required string ContentType { get; set; }

    /// <summary>
    /// 파일 크기 (byte)
    /// </summary>
    [Key(4)]
    public long Size { get; set; }

    /// <summary>
    /// 문서의 태그들
    /// </summary>
    [Key(5)]
    public string[] Tags { get; set; } = [];

    /// <summary>
    /// 문서 생성 시각
    /// </summary>
    [Key(6)]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 문서 업데이트 시각
    /// </summary>
    [Key(7)]
    public DateTime LastUpdatedAt { get; set; }
}
