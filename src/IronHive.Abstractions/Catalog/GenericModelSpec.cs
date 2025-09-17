namespace IronHive.Abstractions.Catalog;

/// <summary>
/// 모든 모델이 공유하는 공통 스펙입니다.
/// </summary>
public record GenericModelSpec : IModelSpec
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
