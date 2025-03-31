namespace IronHive.Abstractions.Memory;

/// <summary>
/// 벡터 스토리지와 관련된 기본 기능을 제공하는 인터페이스입니다.
/// 이 인터페이스는 벡터 컬렉션의 생성, 삭제, 검색 등의 작업을 비동기적으로 수행합니다.
/// </summary>
public interface IVectorStorage : IDisposable
{
    /// <summary>
    /// 해당하는 벡터 컬렉션의 이름을 비동기적으로 반환합니다.
    /// </summary>
    /// <param name="prefix">컬렉션 이름의 접두어 (옵션)</param>
    /// <returns>컬렉션 이름의 나열</returns>
    Task<IEnumerable<string>> ListCollectionsAsync(
        string? prefix = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 이름의 벡터 컬렉션이 존재하는지 비동기적으로 확인합니다.
    /// </summary>
    /// <param name="collectionName">확인할 컬렉션의 이름</param>
    /// <returns>컬렉션이 존재하면 true, 아니면 false</returns>
    Task<bool> CollectionExistsAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 새로운 벡터 컬렉션을 지정된 차원으로 비동기적으로 생성합니다.
    /// </summary>
    /// <param name="collectionName">생성할 컬렉션의 이름</param>
    /// <param name="dimensions">벡터의 차원 수</param>
    Task CreateCollectionAsync(
        string collectionName,
        int dimensions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 이름의 벡터 컬렉션을 비동기적으로 삭제합니다.
    /// </summary>
    /// <param name="collectionName">삭제할 컬렉션의 이름</param>
    Task DeleteCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 조건에 맞는 벡터 레코드들을 지정된 컬렉션에서 비동기적으로 검색합니다.
    /// LastUpdatedAt을 기준으로 내림차순으로 정렬됩니다.
    /// </summary>
    /// <param name="collectionName">검색할 컬렉션의 이름</param>
    /// <param name="limit">검색 결과 최대 개수 (기본값: 20)</param>
    /// <param name="filter">검색 필터 (옵션)</param>
    /// <returns>검색된 벡터 레코드 목록</returns>
    Task<IEnumerable<VectorRecord>> FindVectorsAsync(
        string collectionName,
        int limit = 20,
        VectorRecordFilter? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 벡터 레코드들을 컬렉션에 추가하거나 업데이트합니다.
    /// </summary>
    /// <param name="collectionName">벡터 레코드를 추가 또는 업데이트할 컬렉션의 이름</param>
    /// <param name="vectors">추가 또는 업데이트할 벡터 레코드의 나열</param>
    Task UpsertVectorsAsync(
        string collectionName,
        IEnumerable<VectorRecord> vectors,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 조건에 맞는 벡터 레코드들을 지정된 컬렉션에서 비동기적으로 삭제합니다.
    /// </summary>
    /// <param name="collectionName">벡터 레코드를 삭제할 컬렉션의 이름</param>
    /// <param name="filter">삭제할 벡터 레코드를 선별하는 필터</param>
    Task DeleteVectorsAsync(
        string collectionName,
        VectorRecordFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 기준 벡터와 유사한 벡터 레코드들을 검색합니다.
    /// </summary>
    /// <param name="collectionName">검색할 컬렉션의 이름</param>
    /// <param name="vector">유사도 검색 기준 벡터</param>
    /// <param name="minScore">검색할 벡터의 최소 유사도 점수 (기본값: 0.0f)</param>
    /// <param name="limit">검색 결과 최대 개수 (기본값: 5)</param>
    /// <param name="filter">검색 필터 (옵션)</param>
    /// <returns>유사도가 높은 등급을 나열한 검색 결과</returns>
    Task<IEnumerable<ScoredVectorRecord>> SearchVectorsAsync(
        string collectionName,
        IEnumerable<float> vector,
        float minScore = 0.0f,
        int limit = 5,
        VectorRecordFilter? filter = null,
        CancellationToken cancellationToken = default);
}
