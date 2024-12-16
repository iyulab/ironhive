namespace Raggle.Abstractions.Memory;

/// <summary>
/// IMemoryService 인터페이스는 메모리 컬렉션과 문서의 관리 및 검색 기능을 제공하는 서비스의 인터페이스입니다.
/// </summary>
public interface IRaggleMemory
{
    Task CreateCollectionAsync(
        string collectionName,
        string embedServiceKey,
        string embedModelName,
        CancellationToken cancellationToken = default);

    Task DeleteCollectionAsync(
        string collectionName, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 문서를 업로드하고, 데이터 파이프라인에 따라 처리한 후 메모리에 저장합니다.
    /// </summary>
    Task MemorizeDocumentAsync(
        string collectionName,
        string documentId,
        string fileName,
        Stream content,
        string[] steps,
        IDictionary<string, object>? options = null,
        string[]? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 컬렉션에서 문서를 삭제합니다.
    /// </summary>
    Task UnMemorizeDocumentAsync(
        string collectionName,
        string documentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 컬렉션에서 쿼리와 최소 점수 조건을 바탕으로 문서를 검색합니다.
    /// </summary>
    Task<IEnumerable<ScoredVectorPoint>> GetNearestVectorAsync(
        string collectionName,
        string embedServiceKey,
        string embedModelName,
        string query,
        float minScore = 0,
        int limit = 5,
        MemoryFilter? filter = null,
        CancellationToken cancellationToken = default);
}
