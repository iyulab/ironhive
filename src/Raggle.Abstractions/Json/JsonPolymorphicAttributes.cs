namespace Raggle.Abstractions.Json;

/// <summary>
/// 상위 객체의 판별자 이름을 지정합니다.
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false)]
public class JsonDiscriminatorNameAttribute : Attribute
{
    public string PropertyName { get; }

    public JsonDiscriminatorNameAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }
}

/// <summary>
/// 파생 클래스의 판별자 값을 지정합니다.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class JsonDiscriminatorValueAttribute : Attribute
{
    public string PropertyValue { get; }

    public JsonDiscriminatorValueAttribute(string propertyValue)
    {
        PropertyValue = propertyValue;
    }
}
