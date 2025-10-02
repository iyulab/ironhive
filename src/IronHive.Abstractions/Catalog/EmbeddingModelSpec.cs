namespace IronHive.Abstractions.Catalog;

/// <summary>
/// 임베딩 모델 전용 스펙입니다.
/// </summary>
public sealed record EmbeddingModelSpec : GenericModelSpec
{
    /// <summary>
    /// 단일 요청에서 허용되는 최대 입력 토큰 수.
    /// </summary>
    public int? MaxInputTokens { get; init; }

    /// <summary>
    /// 임베딩 벡터의 길이(차원 수).
    /// </summary>
    public int? Dimensions { get; init; }
}