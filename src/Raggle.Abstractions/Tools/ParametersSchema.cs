using System.Text.Json.Serialization;

namespace Raggle.Abstractions.Tools;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ObjectTypeSchema), "object")]
[JsonDerivedType(typeof(ArrayTypeSchema), "array")]
[JsonDerivedType(typeof(StringTypeSchema), "string")]
[JsonDerivedType(typeof(NumberTypeSchema), "number")]
[JsonDerivedType(typeof(BooleanTypeSchema), "boolean")]
public abstract class TypeSchema 
{
    public string? Description { get; set; }
}

public class ObjectTypeSchema : TypeSchema
{
    public IDictionary<string, TypeSchema>? Properties { get; set; }
    public ICollection<string>? Required { get; set; }
}

public class ArrayTypeSchema : TypeSchema
{

}

public class StringTypeSchema : TypeSchema
{
    public StringFormat? Format { get; set; }
    public ICollection<string>? Enum { get; set; }
}

public class NumberTypeSchema : TypeSchema
{
    public NumberFormat? Format { get; set; }
}

public class BooleanTypeSchema : TypeSchema
{

}

public enum NumberFormat
{
    Int32,
    Int64,
    Float,
    Double,
}

public enum StringFormat
{
    String,
    Date,
    DateTime,
    Time,
    Uri,
    GUID,
}
