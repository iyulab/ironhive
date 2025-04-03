namespace IronHive.Abstractions.Memory;

public class MemoryTarget
{
    /// <summary>
    /// 벡터스토리지의 컬렉션 이름입니다.
    /// </summary>
    public required string CollectionName { get; set; }

    /// <summary>
    /// 임베딩 모델 제공자의 서비스키 입니다.
    /// </summary>
    public required string EmbedProvider { get; set; }

    /// <summary>
    /// 임베딩 모델의 이름입니다.
    /// </summary>
    public required string EmbedModel { get; set; }
}
