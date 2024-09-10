namespace Raggle.Abstractions.Tools;

public class ParametersSchema
{
    public string Type { get; } = "object";

    public IDictionary<string, PropertySchema>? Properties { get; set; }

    public ICollection<string>? Required { get; set; }
}

public class PropertySchema
{
    public required PropertyType Type { get; set; }

    public string? Description { get; set; }
}

public enum PropertyType
{
    String,
    Number,
    Boolean,
    Object,
    Array,
    Null
}
