namespace Raggle.Abstractions.Json;

/// <summary>
/// 상위 객체의 판별자 이름을 지정합니다.
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false)]
public class PolymorphicPropertyNameAttribute : Attribute
{
    public string Name { get; }

    public PolymorphicPropertyNameAttribute(string name)
    {
        Name = name;
    }
}

/// <summary>
/// 파생 클래스의 판별자 값을 지정합니다.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PolymorphicPropertyValueAttribute : Attribute
{
    public string Value { get; }

    public PolymorphicPropertyValueAttribute(string value)
    {
        Value = value;
    }
}
