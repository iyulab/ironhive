using IronHive.Abstractions.Vector;

namespace IronHive.Abstractions.Memory;

/// <summary>
/// 벡터기반 RAG(정보 검색 기반 생성) 메모리 서비스를 정의하는 인터페이스입니다.
/// </summary>
public interface IMemoryService
{
    /// <summary>
    /// 지정한 스토리지와 컬렉션 이름에 해당하는 벡터 컬렉션을 비동기적으로 반환합니다.
    /// </summary>
    Task<IMemoryCollection> GetCollectionAsync(
        string storageName,
        string collectionName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 접두사로 시작하는 벡터 컬렉션 목록을 비동기적으로 반환합니다.
    /// </summary>
    /// <param name="storageName">벡터 스토리지 이름</param>
    /// <param name="prefix">컬렉션 이름의 접두사 (옵션)</param>
    Task<IEnumerable<VectorCollectionInfo>> ListCollectionsAsync(
        string storageName,
        string? prefix = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 이름의 컬렉션이 존재하는지 확인합니다.
    /// </summary>
    /// <param name="storageName">벡터 스토리지 이름</param>
    /// <param name="collectionName">컬렉션 이름</param>
    Task<bool> CollectionExistsAsync(
        string storageName,
        string collectionName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 이름의 새로운 벡터 컬렉션을 비동기적으로 생성합니다.
    /// </summary>
    /// <param name="storageName">벡터 스토리지 이름</param>
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
    /// <param name="storageName">벡터 스토리지 이름</param>
    /// <param name="collectionName">삭제할 컬렉션 이름</param>
    Task DeleteCollectionAsync(
        string storageName,
        string collectionName,
        CancellationToken cancellationToken = default);
}