using IronHive.Abstractions.Vector;

namespace IronHive.Abstractions.Memory;

/// <summary>
/// 벡터기반 RAG(정보 검색 기반 생성) 메모리 서비스를 정의하는 인터페이스입니다.
/// </summary>
public interface IMemoryService
{
    /// <summary>
    /// 메모리 작업자 서비스
    /// </summary>
    IMemoryWorkerService Workers { get; }

    /// <summary>
    /// 지정된 소스를 벡터화시키는 큐에 등록합니다.
    /// </summary>
    /// <param name="source">인덱싱할 소스</param>
    Task QueueIndexSourceAsync(
        IMemorySource source,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 소스를 즉시 벡터화하여 인덱싱합니다.
    /// </summary>
    /// <param name="source">인덱싱할 소스</param>
    Task IndexSourceAsync(
        IMemorySource source,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 소스를 컬렉션에서 제거합니다.
    /// </summary>
    /// <param name="sourceId">제거할 소스의 ID</param>
    Task DeleteSourceAsync(
        string sourceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 쿼리를 기반으로 코사인 유사도를 사용하여 유사한 항목을 검색합니다.
    /// </summary>
    /// <param name="query">검색 질의</param>
    /// <param name="minScore">최소 유사도 점수</param>
    /// <param name="limit">최대 결과 수</param>
    /// <param name="sourceIds">제한할 소스 ID 목록 (옵션)</param>
    Task<VectorSearchResult> SearchSimilarAsync(
        string query,
        float minScore = 0,
        int limit = 5,
        IEnumerable<string>? sourceIds = null,
        CancellationToken cancellationToken = default);
}