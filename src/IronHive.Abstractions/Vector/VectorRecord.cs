using IronHive.Abstractions.Memory;

namespace IronHive.Abstractions.Vector;

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
    /// 벡터 값들의 컬렉션입니다.
    /// </summary>
    public required IEnumerable<float> Vectors { get; set; }

    /// <summary>
    /// 벡터의 원본 내용을 나타내는 객체입니다.
    /// </summary>
    public object? Content { get; set; }

    /// <summary>
    /// 벡터의 원본 소스 정보를 나타냅니다.
    /// </summary>
    public required IMemorySource Source { get; set; }

    /// <summary>
    /// 레코드가 레코드가 생성 또는 마지막으로 갱신된 날짜 및 시간입니다. UTC 기준으로 설정됩니다.
    /// </summary>
    public DateTime LastUpsertedAt { get; set; } = DateTime.UtcNow;
}
