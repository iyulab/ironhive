namespace IronHive.Abstractions.Embedding;

/// <summary>
/// 임베딩 결과를 나타냅니다.
/// </summary>
public class EmbeddingResult
{
    /// <summary>
    /// 임베딩의 인덱스를 가져오거나 설정합니다.
    /// </summary>
    public int? Index { get; set; }

    /// <summary>
    /// 임베딩 벡터 값을 가져오거나 설정합니다.
    /// </summary>
    public IEnumerable<float>? Embedding { get; set; }
}
