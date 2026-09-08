namespace IronHive.Abstractions.Reranking;

/// <summary>
/// 리랭킹 결과를 나타냅니다.
/// </summary>
public class RerankResult
{
    /// <summary>
    /// 원본 <c>documents</c> 목록에서의 인덱스를 가져오거나 설정합니다.
    /// </summary>
    public required int Index { get; set; }

    /// <summary>
    /// 쿼리와의 관련도 점수를 가져오거나 설정합니다. 값이 클수록 관련도가 높습니다.
    /// </summary>
    public required float Score { get; set; }
}
