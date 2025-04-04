using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Memory;

/// <summary>
/// 메모리를 저장할 대상을 나타내는 인터페이스입니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(VectorMemoryTarget), "vector")]
public interface IMemoryTarget
{ }

/// <summary>
/// 메모리 저장 대상의 기본 클래스입니다.
/// </summary>
public abstract class MemoryTargetBase : IMemoryTarget
{ }

/// <summary>
/// 메모리를 저장할 벡터 스토리지를 나타내는 클래스입니다.
/// </summary>
public class VectorMemoryTarget : MemoryTargetBase
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
