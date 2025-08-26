namespace IronHive.Core.Tools;

/// <summary>
/// FuctionTool의 매개변수에 붙여서 DI 컨테이너에서 주입받도록 합니다.
///  Microsoft.AspNetCore.Mvc의 FromServicesAttribute와는 별개입니다.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class FromServicesAttribute : Attribute
{
    public FromServicesAttribute()
    { }
}
