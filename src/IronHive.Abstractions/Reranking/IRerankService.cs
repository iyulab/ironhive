namespace IronHive.Abstractions.Reranking;

/// <summary>
/// 다양한 리랭킹 프로바이더를 통합하여 리랭킹을 수행하는 서비스 인터페이스입니다.
/// </summary>
public interface IRerankService
{
    /// <summary>
    /// 등록된 provider 이름과 <see cref="IDocumentReranker"/>의 딕셔너리입니다.
    /// </summary>
    IReadOnlyDictionary<string, IDocumentReranker> Rerankers { get; }

    /// <summary>
    /// 지정된 프로바이더와 모델을 사용하여 쿼리와 문서 목록 간의 관련도를 재계산합니다.
    /// </summary>
    /// <param name="provider">사용할 공급자의 이름입니다.</param>
    /// <param name="modelId">사용할 모델의 이름 또는 식별자입니다.</param>
    /// <param name="query">기준이 되는 쿼리 문자열입니다.</param>
    /// <param name="documents">관련도를 평가할 문서 목록입니다.</param>
    /// <param name="topN">반환할 상위 결과 개수입니다. null이면 전체 결과를 반환합니다.</param>
    Task<IEnumerable<RerankResult>> RerankAsync(
        string provider,
        string modelId,
        string query,
        IEnumerable<string> documents,
        int? topN = null,
        CancellationToken cancellationToken = default);
}
