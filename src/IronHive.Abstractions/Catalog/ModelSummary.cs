namespace IronHive.Abstractions.Catalog;

/// <summary>
/// AI 모델의 기본 정보를 나타냅니다.
/// </summary>
public class ModelSummary
{
    /// <summary>
    /// 모델을 제공하는 공급자의 식별자입니다.
    /// </summary>
    public required string Provider { get; set; }

    /// <summary>
    /// 모델의 고유 식별자입니다.
    /// </summary>
    public required string ModelId { get; set; }

    /// <summary>
    /// 모델의 사용자 친화적인 표시 이름입니다.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 모델에 대한 간략한 설명입니다.
    /// </summary>
    public string? Description { get; set; }
}
