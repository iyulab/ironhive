namespace IronHive.Abstractions.Embedding;

/// <summary>
/// 임베딩 입력 문자열의 토큰 정보(갯수)를 나타냅니다.
/// </summary>
public class EmbeddingTokens
{
    /// <summary>
    /// 아이템의 인덱스를 가져오거나 설정합니다.
    /// </summary>
    public required int Index { get; set; }

    /// <summary>
    /// 원본 텍스트를 가져오거나 설정합니다.
    /// </summary>
    public required string Text { get; set; }

    /// <summary>
    /// 텍스트의 토큰 수를 가져오거나 설정합니다.
    /// </summary>
    public required int TokenCount { get; set; }
}