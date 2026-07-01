using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Models;

/// <summary>
/// 다형 직렬화를 위한 모델 카드의 기본 인터페이스입니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ModelCard), "model")]
[JsonDerivedType(typeof(LanguageModelCard), "language")]
[JsonDerivedType(typeof(EmbeddingModelCard), "embedding")]
public interface IModelCard
{
    /// <summary>
    /// 모델 식별자(고유 값).
    /// </summary>
    string ModelId { get; }

    /// <summary>
    /// 표시용 모델명. 없으면 <see cref="ModelId"/>를 사용하세요.
    /// </summary>
    string? DisplayName { get; }

    /// <summary>
    /// 모델에 대한 짧은 설명.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// 모델을 소유한 주체(예: 조직, 사용자 등).
    /// </summary>
    string? OwnedBy { get; }

    /// <summary>
    /// 모델이 생성된 시각.
    /// </summary>
    DateTime? CreatedAt { get; }

    /// <summary>
    /// 모델이 마지막으로 갱신된 시각.
    /// </summary>
    DateTime? UpdatedAt { get; }
}
