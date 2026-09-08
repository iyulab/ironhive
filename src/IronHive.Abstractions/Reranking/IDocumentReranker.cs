namespace IronHive.Abstractions.Reranking;

/// <summary>
/// 쿼리와 문서 목록 간의 관련도를 재계산(rerank)하는 기능을 제공하는 서비스 인터페이스입니다.
/// </summary>
public interface IDocumentReranker : IDisposable
{
    /// <summary>
    /// 지정된 모델을 사용하여 쿼리와 문서 목록 간의 관련도를 재계산합니다.
    /// </summary>
    /// <param name="modelId">사용할 모델의 식별자입니다.</param>
    /// <param name="query">기준이 되는 쿼리 문자열입니다.</param>
    /// <param name="documents">관련도를 평가할 문서 목록입니다.</param>
    /// <param name="topN">반환할 상위 결과 개수입니다. null이면 전체 결과를 반환합니다.</param>
    /// <returns>
    /// <paramref name="documents"/>의 원본 인덱스와 관련도 점수를 담은, 점수 내림차순으로 정렬된 결과 목록을 반환합니다.
    /// </returns>
    Task<IEnumerable<RerankResult>> RerankAsync(
        string modelId,
        string query,
        IEnumerable<string> documents,
        int? topN = null,
        CancellationToken cancellationToken = default);
}
