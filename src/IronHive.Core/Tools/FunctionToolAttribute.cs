namespace IronHive.Core.Tools;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class FunctionToolAttribute : Attribute
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public bool RequiresApproval { get; set; } = false;

    public FunctionToolAttribute()
    { }

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
