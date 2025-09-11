using IronHive.Abstractions.Vector;

namespace IronHive.Abstractions.Memory;

/// <summary>
/// 벡터 스토리지를 사용하는 메모리 컬렉션을 나타냅니다.
/// </summary>
public interface IMemoryCollection
{
    /// <summary>
    /// 벡터 스토리지의 이름입니다.
    /// </summary>
    string StorageName { get; }

    /// <summary>
    /// 벡터 스토리지 컬렉션의 이름입니다.
    /// </summary>
    string CollectionName { get; }

    /// <summary>
    /// 사용되는 임베딩 제공자의 이름입니다.
    /// </summary>
    string EmbeddingProvider { get; }

    /// <summary>
    /// 사용되는 임베딩 모델의 이름입니다.
    /// </summary>
    string EmbeddingModel { get; }

    /// <summary>
    /// 지정된 소스를 벡터화시키는 작업에 등록합니다.
    /// </summary>
    /// <param name="queueName">작업 큐 이름</param>
    /// <param name="source">인덱싱할 소스</param>
    Task IndexSourceAsync(
        string queueName,
        IMemorySource source,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 소스를 제거합니다.
    /// </summary>
    /// <param name="sourceId">제거할 소스의 ID</param>
    Task DeindexSourceAsync(
        string sourceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 쿼리와 연관된 항목을 검색합니다.
    /// </summary>
    /// <param name="query">검색 질의</param>
    /// <param name="options">검색 옵션</param>
    Task<VectorSearchResult> SemanticSearchAsync(
        string query,
        MemorySearchOptions? options = null,
        CancellationToken cancellationToken = default);
}
