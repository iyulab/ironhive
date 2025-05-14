namespace IronHive.Core.Tools;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class FunctionToolAttribute : Attribute
{
    public string? Name { get; set; }

    public bool RequiredUserConfirmed { get; set; }

    public FunctionToolAttribute()
    {
        RequiredUserConfirmed = false;
    }

    public FunctionToolAttribute(string? name = null, bool requiredUserConfirmed = false)
    {
        Name = name;
        RequiredUserConfirmed = requiredUserConfirmed;
    }
}
