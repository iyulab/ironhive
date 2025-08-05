using IronHive.Abstractions.Memory;

namespace IronHive.Abstractions.Storages;

/// <summary>
/// 벡터 데이터를 나타내는 레코드 클래스입니다.
/// </summary>
public class VectorRecord
{
    /// <summary>
    /// 벡터의 고유 식별자입니다 (GUID 형식 문자열).
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// 벡터의 원본(출처) 식별자입니다.
    /// </summary>
    public required string SourceId { get; set; }

    /// <summary>
    /// 벡터 값들의 컬렉션입니다.
    /// </summary>
    public required IEnumerable<float> Vectors { get; set; }

    /// <summary>
    /// 벡터 값과 연결된 메모리 소스입니다.
    /// </summary>
    public required IMemorySource Source { get; set; }

    /// <summary>
    /// 벡터와 연관된 페이로드(내용) 정보입니다.
    /// </summary>
    public object? Content { get; set; }

    /// <summary>
    /// 레코드가 생성되었거나 마지막으로 갱신된 날짜 및 시간입니다.
    /// 기본값은 현재 UTC 시간입니다.
    /// </summary>
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}
