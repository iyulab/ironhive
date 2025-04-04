namespace IronHive.Core.Tools;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class FunctionToolAttribute : Attribute
{
    public string? Name { get; set; }

    public bool RequiresApproval { get; set; }

    public FunctionToolAttribute() 
    {
        RequiresApproval = false;
    }

    public FunctionToolAttribute(string? name = null, bool requiresApproval = false)
    {
        Name = name;
        RequiresApproval = requiresApproval;
    }
}
