namespace IronHive.Abstractions.Storages;

/// <summary>
/// 벡터 검색 결과를 나타내는 클래스입니다.
/// </summary>
public class VectorSearchResult
{
    /// <summary>
    /// 검색이 수행된 컬렉션의 이름입니다.
    /// </summary>
    public required string CollectionName { get; set; }

    /// <summary>
    /// 검색에 사용된 질의(query) 문자열입니다.
    /// </summary>
    public string? SearchQuery { get; set; }

    /// <summary>
    /// 검색 결과로 반환된 벡터와 해당 점수들의 목록입니다.
    /// </summary>
    public required IEnumerable<ScoredVectorRecord> ScoredVectors { get; set; }
}