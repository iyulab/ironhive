using IronHive.Abstractions.Memory;

namespace IronHive.Abstractions.Storages;

/// <summary>
/// 점수(score)를 포함하는 벡터 정보를 나타내는 클래스입니다.
/// </summary>
public class ScoredVectorRecord
{
    /// <summary>
    /// 벡터의 고유 식별자입니다.
    /// </summary>
    public required string VectorId { get; set; }

    /// <summary>
    /// 벡터의 점수입니다. (예: 유사도, 중요도 등)
    /// 0 부터 1 사이의 값을 가지며, 1에 가까울수록 높은 유사도를 나타냅니다.
    /// </summary>
    public required float Score { get; set; }

    /// <summary>
    /// 해당 벡터와 연관된 메모리 소스입니다.
    /// </summary>
    public required IMemorySource Source { get; set; }

    /// <summary>
    /// 벡터와 연결된 페이로드(내용) 정보입니다.
    /// </summary>
    public object? Content { get; set; }

    /// <summary>
    /// 벡터가 마지막으로 갱신된 날짜 및 시간입니다.
    /// </summary>
    public required DateTime LastUpdatedAt { get; set; }
}
