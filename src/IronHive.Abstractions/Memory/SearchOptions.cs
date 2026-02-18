namespace IronHive.Abstractions.Memory;

/// <summary>
/// 검색 모드를 정의하는 열거형입니다.
/// </summary>
public enum SearchMode
{
    CosineSimilarity // 코사인 유사도를 기준으로 검색을 수행합니다.
}

/// <summary>
/// 검색 동작에 필요한 옵션을 지정하는 설정 클래스입니다.
/// </summary>
public sealed record SearchOptions
{
    /// <summary>
    /// 검색 모드 (기본값: CosineSimilarity).
    /// </summary>
    public SearchMode Mode { get; init; } = SearchMode.CosineSimilarity;

    /// <summary>
    /// 검색 결과로 허용되는 최소 점수 (기본값: 0).
    /// </summary>
    public float MinScore { get; init; }

    /// <summary>
    /// 검색 결과의 최대 개수 (기본값: 5).
    /// </summary>
    public int Limit { get; init; } = 5;

    /// <summary>
    /// 검색 대상이 되는 소스의 ID 목록 (null이면 모든 소스를 대상으로 함).
    /// </summary>
    public IEnumerable<string>? SourceIds { get; init; }
}
