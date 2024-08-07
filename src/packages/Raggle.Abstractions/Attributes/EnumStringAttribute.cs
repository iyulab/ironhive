using System.Reflection;

namespace Raggle.Abstractions.Attributes;

public class EnumStringAttribute : Attribute
{
    public string Value { get; }

    public EnumStringAttribute(string value)
    {
        Value = value;
    }
}

public static class EnumExtensions
{
    public static string GetValue(this Enum model)
    {
        var field = model.GetType().GetField(model.ToString());
        var attribute = field?.GetCustomAttribute<EnumStringAttribute>();

        return attribute?.Value ?? model.ToString();
    }
}
