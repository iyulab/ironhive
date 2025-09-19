namespace IronHive.Abstractions.Vector;

/// <summary>
/// 벡터 데이터를 나타내는 레코드 클래스입니다.
/// </summary>
public class VectorRecord
{
    /// <summary>
    /// 벡터의 고유 식별자입니다.
    /// </summary>
    public required string VectorId { get; set; }

    /// <summary>
    /// 벡터 원본 소스의 고유 식별자입니다.
    /// </summary>
    public required string SourceId { get; set; }

    /// <summary>
    /// 벡터 값들을 나타내는 실수(float) 배열입니다.
    /// </summary>
    public required float[] Vectors { get; set; }

    /// <summary>
    /// 벡터와 같이 저장되는 내용입니다.
    /// </summary>
    public required IDictionary<string, object?> Payload { get; set; } 
        = new Dictionary<string, object?>(StringComparer.Ordinal);

    /// <summary>
    /// 레코드가 레코드가 생성 또는 마지막으로 갱신된 날짜 및 시간입니다. UTC 기준으로 설정됩니다.
    /// </summary>
    public DateTime LastUpsertedAt { get; set; } = DateTime.UtcNow;
}
