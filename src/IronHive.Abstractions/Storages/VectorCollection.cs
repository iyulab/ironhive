namespace IronHive.Abstractions.Storages;

/// <summary>
/// 벡터 스토리지의 컬렉션을 나타내는 클래스입니다.
/// </summary>
public class VectorCollection
{
    /// <summary>
    /// 벡터 컬렉션의 이름
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 컬렉션의 벡터 차원 수
    /// </summary>
    public required long Dimensions { get; set; }

    /// <summary>
    /// 임베딩 제공자의 이름
    /// </summary>
    public required string EmbeddingProvider { get; set; }

    /// <summary>
    /// 임베딩 모델의 이름
    /// </summary>
    public required string EmbeddingModel { get; set; }
}
