namespace IronHive.Abstractions.Models;

/// <summary>
/// Represents a model descriptor.
/// </summary>
public class ModelDescriptor
{
    /// <summary>
    /// 모델 제공자의 키 값입니다.
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// 모델의 고유 식별자 입니다.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// 모델에 대한 설명입니다.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 모델의 표시 이름입니다.
    /// </summary>
    public string? Display { get; set; }

    /// <summary>
    /// 모델의 소유자 또는 기관입니다.
    /// </summary>
    public string? Owner { get; set; }

    /// <summary>
    /// 모델이 생성된 날짜 및 시간입니다.
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}
