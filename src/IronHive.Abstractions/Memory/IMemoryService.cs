using IronHive.Abstractions.Vector;

namespace IronHive.Abstractions.Memory;

/// <summary>
/// 벡터기반 RAG(정보 검색 기반 생성) 메모리 서비스를 정의하는 인터페이스입니다.
/// </summary>
public interface IMemoryService
{
    /// <summary>
    /// 설정된 파이프라인을 다시 재구성 합니다.
    /// </summary>
    void SetPipeline(PipelineBuildDelegate configure);

    /// <summary>
    /// 지정된 큐 스토리지 이름으로부터 메모리 작업자 서비스를 비동기적으로 생성합니다.
    /// </summary>
    IMemoryWorkerService CreateWorkers(MemoryWorkerConfig config);

    /// <summary>
    /// 지정된 접두사로 시작하는 벡터 컬렉션 목록을 비동기적으로 반환합니다.
    /// </summary>
    /// <param name="storageName">벡터 스토리지 이름 또는 식별자</param>
    Task<IEnumerable<VectorCollection>> ListCollectionsAsync(
        string storageName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 이름의 컬렉션이 존재하는지 확인합니다.
    /// </summary>
    /// <param name="storageName">벡터 스토리지 이름 또는 식별자</param>
    /// <param name="collectionName">컬렉션 이름</param>
    Task<bool> CollectionExistsAsync(
        string storageName,
        string collectionName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 이름의 컬렉션의 정보를 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="storageName">벡터 스토리지 이름 또는 식별자</param>
    /// <param name="collectionName">컬렉션 이름</param>
    Task<VectorCollection?> GetCollectionInfoAsync(
        string storageName,
        string collectionName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 이름의 새로운 벡터 컬렉션을 비동기적으로 생성합니다.
    /// </summary>
    /// <param name="storageName">벡터 스토리지 이름 또는 식별자</param>
    /// <param name="collectionName">컬렉션 이름</param>
    /// <param name="embeddingProvider">임베딩 제공자</param>
    /// <param name="embeddingModel">임베딩 모델</param>
    Task CreateCollectionAsync(
        string storageName,
        string collectionName,
        string embeddingProvider,
        string embeddingModel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 이름의 벡터 컬렉션을 비동기적으로 삭제합니다.
    /// </summary>
    /// <param name="storageName">벡터 스토리지 이름 또는 식별자</param>
    /// <param name="collectionName">삭제할 컬렉션 이름</param>
    Task DeleteCollectionAsync(
        string storageName,
        string collectionName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 소스를 벡터화시키는 작업 큐에 등록합니다.
    /// </summary>
    /// <param name="queueName">작업 큐 이름 또는 식별자</param>
    /// <param name="source">인덱싱할 소스</param>
    /// <param name="target">벡터화된 데이터를 저장할 대상</param>
    Task QueueIndexSourceAsync(
        string queueName,
        IMemorySource source,
        IMemoryTarget target,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 소스를 즉시 벡터화하여 인덱싱합니다.
    /// </summary>
    /// <param name="source">인덱싱할 소스</param>
    /// <param name="target">벡터화된 데이터를 저장할 대상</param>
    Task IndexSourceAsync(
        IMemorySource source,
        IMemoryTarget target,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 소스를 컬렉션에서 제거합니다.
    /// </summary>
    /// <param name="storageName">벡터 스토리지 이름 또는 식별자</param>
    /// <param name="collectionName">대상 컬렉션 이름</param>
    /// <param name="sourceId">제거할 소스 ID</param>
    Task DeleteSourceAsync(
        string storageName,
        string collectionName,
        string sourceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 쿼리를 기반으로 코사인 유사도를 사용하여 유사한 항목을 검색합니다.
    /// </summary>
    /// <param name="storageName">벡터 스토리지 이름 또는 식별자</param>
    /// <param name="collectionName">검색할 컬렉션 이름</param>
    /// <param name="query">검색 질의</param>
    /// <param name="minScore">최소 유사도 점수</param>
    /// <param name="limit">최대 결과 수</param>
    /// <param name="sourceIds">제한할 소스 ID 목록 (옵션)</param>
    Task<VectorSearchResult> SearchSimilarAsync(
        string storageName,
        string collectionName,
        string query,
        float minScore = 0,
        int limit = 5,
        IEnumerable<string>? sourceIds = null,
        CancellationToken cancellationToken = default);
}