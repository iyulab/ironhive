using System.Reflection;
using System.Runtime.CompilerServices;
using Json.Schema;
using Json.Schema.Generation;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using JsonSchemaNet = Json.Schema.JsonSchema;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace IronHive.Abstractions.Json;

/// <summary>
/// .NET 타입으로부터 JSON 스키마를 생성하는 팩토리 클래스입니다.
/// </summary>
public static class JsonSchemaFactory
{
    /// <summary>
    /// Json.Schema 라이브러리를 사용하여 .NET 제네릭 타입으로부터 JSON 스키마를 생성합니다.
    /// </summary>
    [Obsolete("대신 CreateFrom<T>() 메서드를 사용하세요. 이 메서드는 향후 버전에서 제거될 예정입니다.")]
    public static JsonSchemaNet CreateNew<T>()
    {
        var schema = new JsonSchemaBuilder()
            .FromType<T>()
            .Build();
        return schema;
    }

    /// <summary>
    /// 지정한 .NET 타입 T로부터 JSON 스키마를 생성합니다.
    /// </summary>
    /// <param name="description">스키마 설명 (선택 사항)</param>
    public static JsonSchema CreateFrom<T>(string? description = null)
    {
        return CreateFrom(typeof(T), description);
    }

    /// <summary>
    /// 지정한 System.Type 객체로부터 JSON 스키마를 생성합니다.
    /// </summary>
    /// <param name="type">.NET 타입 정보</param>
    /// <param name="description">스키마 설명 (선택 사항)</param>
    public static JsonSchema CreateFrom(Type type, string? description = null)
    {
        // boolean 타입 처리
        if (type == typeof(bool))
            return new BooleanJsonSchema(description);

        // 정수형 타입 처리
        if (type == typeof(byte))
            return new IntegerJsonSchema(description) { Format = "byte" };
        if (type == typeof(sbyte))
            return new IntegerJsonSchema(description) { Format = "sbyte" };
        if (type == typeof(short))
            return new IntegerJsonSchema(description) { Format = "short" };
        if (type == typeof(ushort))
            return new IntegerJsonSchema(description) { Format = "ushort" };
        if (type == typeof(int))
            return new IntegerJsonSchema(description) { Format = "int32" };
        if (type == typeof(uint))
            return new IntegerJsonSchema(description) { Format = "uint32" };
        if (type == typeof(long))
            return new IntegerJsonSchema(description) { Format = "int64" };
        if (type == typeof(ulong))
            return new IntegerJsonSchema(description) { Format = "uint64" };

        // 실수형 타입 처리
        if (type == typeof(float))
            return new NumberJsonSchema(description) { Format = "float" };
        if (type == typeof(double))
            return new NumberJsonSchema(description) { Format = "double" };
        if (type == typeof(decimal))
            return new NumberJsonSchema(description) { Format = "decimal" };

        // 문자열 타입 처리
        if (type == typeof(char))
            return new StringJsonSchema(description) { MaxLength = 1, MinLength = 1 };
        if (type == typeof(string))
            return new StringJsonSchema(description);
        if (type == typeof(TimeSpan))
            return new StringJsonSchema(description) { Format = "duration" };
        if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
            return new StringJsonSchema(description) { Format = "date-time" };
        if (type == typeof(Uri))
            return new StringJsonSchema(description) { Format = "uri" };
        if (type == typeof(Guid))
            return new StringJsonSchema(description) { Format = "guid" };
        if (type.IsEnum)
            return new StringJsonSchema(description) { Enum = Enum.GetNames(type) };

        // 배열 또는 컬렉션 타입 처리
        if (type.IsArray)
        {
            var genericType = type.GetElementType()
                ?? throw new ArgumentException("배열 타입은 요소 타입이 있어야 합니다.", nameof(type));
            var items = CreateFrom(genericType);
            return new ArrayJsonSchema(description) { Items = items };
        }
        if (IsGenericArray(type))
        {
            var genericType = type.GetGenericArguments()[0];
            var items = CreateFrom(genericType);
            return new ArrayJsonSchema(description) { Items = items };
        }
        if (IsTuple(type))
        {
            var genericTypes = type.GetGenericArguments();
            var items = genericTypes.Select(t => CreateFrom(t)).ToArray();
            return new ArrayJsonSchema(description) { Items = items };
        }

        // 객체 타입 처리 (딕셔너리, 클래스, 구조체, 레코드 등)
        if (IsDictionary(type))
        {
            var valueType = type.GetGenericArguments()[1];
            var additionalProperties = CreateFrom(valueType);
            return new ObjectJsonSchema
            {
                Description = description,
                AdditionalProperties = additionalProperties
            };
        }
        if (IsComplexObject(type))
        {
            var properties = new Dictionary<string, JsonSchema>();
            var required = new List<string>();

            foreach (var prop in type.GetProperties())
            {
                var propType = prop.PropertyType;
                var propDescription = prop.GetCustomAttribute<DescriptionAttribute>()?.Description;
                var propSchema = CreateFrom(propType, propDescription);
                properties.Add(prop.Name, propSchema);

                if (IsRequiredProperty(prop))
                    required.Add(prop.Name);
            }

            return new ObjectJsonSchema
            {
                Description = description,
                Properties = properties,
                Required = required.Count > 0 ? required.ToArray() : null
            };
        }

        throw new ArgumentException("지원하지 않는 타입입니다.", nameof(type));
    }

    /// <summary>
    /// 제네릭 배열(리스트/컬렉션 등) 타입 여부 확인
    /// </summary>
    private static bool IsGenericArray(Type type)
    {
        if (!type.IsGenericType)
            return false;

        var gType = type.GetGenericTypeDefinition();
        return gType == typeof(IEnumerable<>) || gType == typeof(ICollection<>) || gType == typeof(IList<>)
            || gType == typeof(List<>) || gType == typeof(IReadOnlyList<>) || gType == typeof(IReadOnlyCollection<>);
    }

    /// <summary>
    /// 튜플 타입 여부 확인
    /// </summary>
    private static bool IsTuple(Type type)
    {
        var interfaces = type.GetInterfaces();
        return interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITuple));
    }

    /// <summary>
    /// 딕셔너리 타입 여부 확인
    /// </summary>
    private static bool IsDictionary(Type type)
    {
        return type.GetInterfaces().Any(i =>
            i.IsGenericType &&
            i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
    }

    /// <summary>
    /// 복합 객체(클래스, 구조체, 레코드) 타입 여부 확인
    /// </summary>
    private static bool IsComplexObject(Type type)
    {
        if (type.IsClass && !type.IsAbstract && !type.IsInterface)
            return true;

        if (type.IsValueType && !type.IsPrimitive && !type.IsEnum)
            return true;

        return false;
    }

    /// <summary>
    /// 해당 속성이 필수인지 확인
    /// </summary>
    private static bool IsRequiredProperty(PropertyInfo prop)
    {
        if (prop.PropertyType.IsValueType && Nullable.GetUnderlyingType(prop.PropertyType) == null)
            return true;

        if (prop.GetCustomAttribute<RequiredAttribute>() != null)
            return true;

        if (prop.GetCustomAttribute<RequiredMemberAttribute>() != null)
            return true;

        return false;
    }
}
