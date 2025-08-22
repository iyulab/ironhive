namespace IronHive.Core.Tools;

/// <summary>
/// 메서드에 부착할 수 있는 도구 속성(Attribute)입니다.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class FunctionToolAttribute : Attribute
{
    /// <summary>
    /// 도구의 이름입니다.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 도구에 대한 설명입니다.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 도구 사용 시 승인이 필요한지 여부를 나타냅니다. 기본값은 false입니다.
    /// </summary>
    public bool RequiresApproval { get; set; } = false;

    /// <summary>
    /// 도구 사용시 제한 시간입니다. 초 단위로 지정하며, 기본값은 1분(60초)입니다.
    /// </summary>
    public long Timeout { get; set; } = 60;

    /// <summary>
    /// 기본 생성자입니다.
    /// </summary>
    public FunctionToolAttribute()
    { }

    /// <summary>
    /// 이름, 설명을 지정하는 생성자입니다.
    /// </summary>
    /// <param name="name">도구의 이름</param>
    /// <param name="description">도구의 설명</param>
    public FunctionToolAttribute(
        string? name = null,
        string? description = null)
    {
        Name = name;
        Description = description;
    }
}
