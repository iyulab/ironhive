using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Memory;

/// <summary>
/// 메모리를 저장할 대상을 나타내는 객체입니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(VectorMemoryTarget), "vector")]
public interface IMemoryTarget
{ }

/// <inheritdoc />
public abstract class MemoryTargetBase : IMemoryTarget
{ }

/// <summary>
/// 메모리를 저장할 벡터 스토리지를 나타내는 클래스입니다.
/// </summary>
public class VectorMemoryTarget : MemoryTargetBase
{
    /// <summary>
    /// 등록된 스토리지의 이름입니다.
    /// </summary>
    public required string StorageName { get; set; }

    /// <summary>
    /// 벡터스토리지의 컬렉션 이름입니다.
    /// </summary>
    public required string CollectionName { get; set; }

    /// <summary>
    /// 임베딩 제공자
    /// </summary>
    public required string EmbeddingProvider { get; set; }

    /// <summary>
    /// 임베딩 모델 이름
    /// </summary>
    public required string EmbeddingModel { get; set; }
}