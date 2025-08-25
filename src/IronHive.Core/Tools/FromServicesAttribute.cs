namespace IronHive.Core.Tools;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class FromServicesAttribute : Attribute
{
    public FromServicesAttribute()
    { }
}
