using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// 모델이 도구를 호출할지, 어떤 도구를 호출할지를 제어합니다.
/// <see cref="MessageGenerationRequest.Tools"/>가 비어있지 않을 때만 의미가 있습니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AutoToolChoice), "auto")]
[JsonDerivedType(typeof(NoneToolChoice), "none")]
[JsonDerivedType(typeof(RequiredToolChoice), "required")]
[JsonDerivedType(typeof(FunctionToolChoice), "function")]
public abstract class ToolChoice
{
    /// <summary>
    /// 모델이 도구 호출 여부를 자유롭게 결정합니다. 값을 지정하지 않은 것과 동일합니다.
    /// </summary>
    public static readonly ToolChoice Auto = new AutoToolChoice();

    /// <summary>
    /// 도구 목록이 있어도 도구 호출을 억제합니다.
    /// </summary>
    public static readonly ToolChoice None = new NoneToolChoice();

    /// <summary>
    /// 최소 한 개 이상의 도구 호출을 강제합니다.
    /// </summary>
    public static readonly ToolChoice Required = new RequiredToolChoice();

    /// <summary>
    /// 지정된 이름들 중 하나의 도구 호출을 강제합니다.
    /// </summary>
    /// <param name="names">강제할 도구 이름(들). 하나 이상 지정해야 합니다.</param>
    public static ToolChoice Function(params string[] names) =>
        new FunctionToolChoice(names);
}

/// <summary>
/// 모델이 도구 호출 여부를 자유롭게 결정합니다.
/// </summary>
public sealed class AutoToolChoice : ToolChoice
{ }

/// <summary>
/// 도구 목록이 있어도 도구 호출을 억제합니다.
/// </summary>
public sealed class NoneToolChoice : ToolChoice
{ }

/// <summary>
/// 최소 한 개 이상의 도구 호출을 강제합니다.
/// </summary>
public sealed class RequiredToolChoice : ToolChoice
{ }

/// <summary>
/// 지정된 이름들 중 하나의 도구 호출을 강제합니다.
/// </summary>
public sealed class FunctionToolChoice : ToolChoice
{
    /// <summary>
    /// 강제할 도구 이름들입니다. 하나 이상을 포함합니다.
    /// </summary>
    public IReadOnlyCollection<string> Names { get; }

    /// <summary>
    /// 강제할 도구 이름들을 지정하여 객체를 생성합니다.
    /// </summary>
    /// <param name="names">강제할 도구 이름(들). 하나 이상 지정해야 합니다.</param>
    public FunctionToolChoice(IEnumerable<string> names)
    {
        ArgumentNullException.ThrowIfNull(names);
        Names = names.ToArray();
        if (Names.Count == 0)
            throw new ArgumentException("At least one function name is required.", nameof(names));
    }
}
