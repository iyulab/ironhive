namespace IronHive.Core.Tools;

/// <summary>
/// 메서드에 부착할 수 있는 기능 도구 속성(Attribute)입니다.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class FunctionToolAttribute : Attribute
{
    /// <summary>
    /// 기능의 이름입니다.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 기능에 대한 설명입니다.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 기능 사용 시 승인이 필요한지 여부를 나타냅니다. 기본값은 false입니다.
    /// </summary>
    public bool RequiresApproval { get; set; } = false;

    /// <summary>
    /// 기본 생성자입니다.
    /// </summary>
    public FunctionToolAttribute()
    { }

    /// <summary>
    /// 이름, 설명, 승인 여부를 지정하는 생성자입니다.
    /// </summary>
    /// <param name="name">기능의 이름</param>
    /// <param name="description">기능의 설명</param>
    /// <param name="requiresApproval">승인이 필요한지 여부</param>
    public FunctionToolAttribute(
        string? name = null,
        string? description = null,
        bool requiresApproval = false)
    {
        Name = name;
        Description = description;
        RequiresApproval = requiresApproval;
    }
}
