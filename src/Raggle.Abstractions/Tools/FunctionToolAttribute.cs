namespace Raggle.Abstractions.Tools;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class FunctionToolAttribute : Attribute
{
    public string? Name { get; private set; }
    public string? Description { get; private set; }

    public FunctionToolAttribute(string? name = null, string? description = null)
    {
        Name = name;
        Description = description;
    }
}
