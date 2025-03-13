namespace IronHive.Core.ChatCompletion;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class FunctionToolAttribute : Attribute
{
    public string? Name { get; set; }

    public FunctionToolAttribute() { }

    public FunctionToolAttribute(
        string? name = null)
    {
        Name = name;
    }
}
