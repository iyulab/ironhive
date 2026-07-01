namespace IronHive.Abstractions.Models;

/// <summary>
/// 모든 모델이 공유하는 공통 모델 카드입니다.
/// </summary>
public record ModelCard : IModelCard
{
    /// <inheritdoc />
    public required string ModelId { get; init; }

    /// <inheritdoc />
    public string? DisplayName { get; init; }

    /// <inheritdoc />
    public string? Description { get; init; }

    /// <inheritdoc />
    public string? OwnedBy { get; init; }

    /// <inheritdoc />
    public DateTime? CreatedAt { get; init; }

    /// <inheritdoc />
    public DateTime? UpdatedAt { get; init; }
}
