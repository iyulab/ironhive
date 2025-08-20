namespace IronHive.Abstractions.Json;

/// <summary>
/// 상위 객체의 판별자 이름을 지정합니다.
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false)]
public class JsonPolymorphicNameAttribute : Attribute
{
    public string Name { get; }

    public JsonPolymorphicNameAttribute(string name)
    {
        Name = name;
    }
}

/// <summary>
/// 파생 클래스의 판별자 값을 지정합니다.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class JsonPolymorphicValueAttribute : Attribute
{
    public string Value { get; }

    public JsonPolymorphicValueAttribute(string value)
    {
        Value = value;
    }
}
