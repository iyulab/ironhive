namespace IronHive.Core.Tools;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class FromToolOptionsAttribute : Attribute
{ 
    public FromToolOptionsAttribute()
    { }
}