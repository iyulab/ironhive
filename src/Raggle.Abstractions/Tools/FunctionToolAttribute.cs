namespace Raggle.Abstractions.Tools;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class FunctionToolAttribute : Attribute
{
    public string? Name { get; set; }
    public string? Description { get; set; }

    public FunctionToolAttribute() { }

    public FunctionToolAttribute(
        string? name = null,
        string? description = null)
    {
        Name = name;
        Description = description;
    }
}
