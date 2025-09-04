namespace IronHive.Core.Tools;

/// <summary>
/// Function 호출 시 매개변수를 지정된 도구(툴) 옵션에서 주입하도록 지정하는 특성입니다.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class FromOptionsAttribute : Attribute
{ 
    public FromOptionsAttribute()
    { }
}