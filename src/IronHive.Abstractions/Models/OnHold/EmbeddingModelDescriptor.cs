namespace IronHive.Abstractions.Models.OnHold;

/// <summary>
/// 임베딩 모델에 대한 정보입니다.
/// </summary>
public class EmbeddingModelDescriptor
{
    /// <summary>
    /// 모델 제공자의 키 값입니다.
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// 임베딩 모델의 이름(또는 식별자)입니다.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// 임베딩 모델에 대한 설명입니다.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 임베딩 모델의 표시 이름입니다.
    /// </summary>
    public string? Display { get; set; }

    /// <summary>
    /// 모델이 지원하는 최대 입력 토큰 수입니다.
    /// </summary>
    public int? InputMaxTokens { get; set; }

    /// <summary>
    /// 모델이 출력하는 임베딩 차원 수입니다.
    /// </summary>
    public int? OutputDimensions { get; set; }

    /// <summary>
    /// 모델의 생성 일자입니다.
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}
