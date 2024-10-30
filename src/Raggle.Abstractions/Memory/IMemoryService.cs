using Raggle.Abstractions.Memory.Document;
using Raggle.Abstractions.Memory.Vector;

namespace Raggle.Abstractions.Memory;

/// <summary>
/// IMemoryService 인터페이스는 메모리 컬렉션과 문서의 관리 및 검색 기능을 제공하는 서비스의 인터페이스입니다.
/// </summary>
public interface IMemoryService
{
    /// <summary>
    /// 현재 존재하는 모든 컬렉션의 이름 목록을 가져옵니다.
    /// </summary>
    /// <param name="cancellationToken">작업 취소를 위한 토큰</param>
    /// <returns>컬렉션 이름 목록을 담은 IEnumerable 컬렉션</returns>
    Task<IEnumerable<string>> GetCollectionListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 이름과 벡터 크기로 새로운 메모리 컬렉션을 생성합니다.
    /// </summary>
    /// <param name="collectionName">생성할 컬렉션의 이름</param>
    /// <param name="vectorSize">벡터 크기</param>
    /// <param name="cancellationToken">작업 취소를 위한 토큰</param>
    /// <returns>비동기 작업 Task</returns>
    Task CreateCollectionAsync(string collectionName, ulong vectorSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 이름의 메모리 컬렉션을 삭제합니다.
    /// </summary>
    /// <param name="collectionName">삭제할 컬렉션의 이름</param>
    /// <param name="cancellationToken">작업 취소를 위한 토큰</param>
    /// <returns>비동기 작업 Task</returns>
    Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 컬렉션에서 필터 조건에 맞는 문서들을 검색합니다.
    /// </summary>
    /// <param name="collectionName">검색할 컬렉션의 이름</param>
    /// <param name="filter">적용할 필터 조건</param>
    /// <param name="cancellationToken">작업 취소를 위한 토큰</param>
    /// <returns>검색된 문서 요약들의 IEnumerable 컬렉션</returns>
    Task<IEnumerable<DocumentRecord>> FindDocumentsAsync(string collectionName, MemoryFilter? filter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 컬렉션에 문서를 업로드하고, 파일 이름 및 내용, 태그 정보를 처리합니다.
    /// </summary>
    /// <param name="collectionName">컬렉션의 이름</param>
    /// <param name="documentId">문서 ID</param>
    /// <param name="fileName">파일 이름</param>
    /// <param name="content">파일 내용의 스트림</param>
    /// <param name="tags">문서에 부착할 태그 배열</param>
    /// <param name="cancellationToken">작업 취소를 위한 토큰</param>
    /// <returns>업로드된 문서 요약 객체</returns>
    Task<DocumentRecord> UploadDocumentAsync(string collectionName, string documentId, string fileName, Stream content, string[]? tags = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 문서를 기억하게 하여, 데이터 파이프라인 처리를 위한 단계를 추가하고 업로드 요청을 처리합니다.
    /// </summary>
    /// <param name="collectionName">컬렉션의 이름</param>
    /// <param name="documentId">문서 ID</param>
    /// <param name="steps">데이터 파이프라인 단계 배열</param>
    /// <param name="uploadRequest">업로드 요청 세부사항</param>
    /// <param name="cancellationToken">작업 취소를 위한 토큰</param>
    /// <returns>데이터 파이프라인 객체</returns>
    Task<DataPipeline> MemorizeDocumentAsync(string collectionName, string documentId, string[] steps, UploadRequest? uploadRequest = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 컬렉션에서 문서를 기억 해제합니다.
    /// </summary>
    /// <param name="collectionName">컬렉션의 이름</param>
    /// <param name="documentId">문서 ID</param>
    /// <param name="cancellationToken">작업 취소를 위한 토큰</param>
    Task UnMemorizeDocumentAsync(string collectionName, string documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 컬렉션에서 쿼리와 최소 점수 조건을 바탕으로 문서를 검색합니다.
    /// </summary>
    /// <param name="collectionName">검색할 컬렉션의 이름</param>
    /// <param name="query">검색 쿼리</param>
    /// <param name="minScore">검색 결과의 최소 점수</param>
    /// <param name="limit">검색 결과 제한 수</param>
    /// <param name="filter">적용할 필터 조건</param>
    /// <param name="cancellationToken">작업 취소를 위한 토큰</param>
    Task SearchDocumentMemoryAsync(string collectionName, string query, float minScore = 0, ulong limit = 5, MemoryFilter? filter = null, CancellationToken cancellationToken = default);
}
