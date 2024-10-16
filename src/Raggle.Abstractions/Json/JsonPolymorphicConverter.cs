using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Raggle.Abstractions.Json;

/// <summary>
/// JSON String의 다형성을 지원하는 컨버터입니다.
/// </summary>
public class JsonPolymorphicConverter<T> : JsonConverter<T> where T : class
{
    private const string DefaultDiscriminatorName = "type";
    private readonly string _discriminatorName;

    private static readonly Lazy<Dictionary<string, Type>> _cachedTypeMapping = new(() => GetTypeMapping());
    private readonly Dictionary<string, Type> _typeMapping;

    /// <summary>
    /// 사용자 지정 판별자 이름을 사용하는 생성자입니다.
    /// 판별자 이름은 <see cref="JsonDiscriminatorNameAttribute"/>로 지정된 속성 이름이거나 기본값인 "type"입니다.
    /// 판별자 값은 <see cref="JsonDiscriminatorValueAttribute"/>로 지정된 속성 값입니다.
    /// </summary>
    public JsonPolymorphicConverter()
    {
        var discriminatorNameAttr = typeof(T).GetCustomAttribute<JsonDiscriminatorNameAttribute>();
        _discriminatorName = discriminatorNameAttr?.PropertyName ?? DefaultDiscriminatorName;
        _typeMapping = _cachedTypeMapping.Value;
    }

    /// <summary>
    /// 사용자 지정 판별자 이름 및 타입 매핑을 사용하는 생성자입니다.
    /// </summary>
    /// <param name="discriminatorName">JSON 타입 판별자 속성 이름</param>
    /// <param name="typeMapping">파생 클래스와 판별자 값의 매핑</param>
    public JsonPolymorphicConverter(
        string? discriminatorName = null,
        Dictionary<string, Type>? typeMapping = null)
    {
        _discriminatorName = discriminatorName ?? DefaultDiscriminatorName;
        _typeMapping = typeMapping ?? _cachedTypeMapping.Value;
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Load the JSON document
        using (var jsonDoc = JsonDocument.ParseValue(ref reader))
        {
            if (!jsonDoc.RootElement.TryGetProperty(_discriminatorName, out var discriminatorProperty))
                throw new JsonException($"Missing discriminator property '{_discriminatorName}'.");

            var discriminatorValue = discriminatorProperty.GetString();
            if (string.IsNullOrEmpty(discriminatorValue) || !_typeMapping.ContainsKey(discriminatorValue))
                throw new JsonException($"Unknown discriminator value '{discriminatorValue}', Are you missing a type mapping?");

            var jsonObject = jsonDoc.RootElement.GetRawText();
            var targetType = _typeMapping[discriminatorValue];
            var instance = JsonSerializer.Deserialize(jsonObject, targetType, options)
                ?? throw new JsonException($"Failed to deserialize JSON to '{targetType.Name}'.");

            return (T)instance;
        }
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var type = value.GetType();
        var discriminatorValue = _typeMapping.FirstOrDefault(x => x.Value == type).Key;

        writer.WriteStartObject();

        // 디스크리미네이터 작성
        if (!string.IsNullOrEmpty(discriminatorValue))
            writer.WriteString(_discriminatorName, discriminatorValue);

        // 모든 속성 작성
        foreach (var property in JsonSerializer.SerializeToElement(value, type, options).EnumerateObject())
        {
            property.WriteTo(writer);
        }

        writer.WriteEndObject();
    }

    private static Dictionary<string, Type> GetTypeMapping()
    {
        // 대소문자 구분 없이 딕셔너리를 만듭니다.
        var typeMapping = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        var derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly =>
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // 어플리케이션 중단 없이 로드할 수 없는 타입을 건너뜁니다.
                    Debug.WriteLine($"Error loading types from assembly '{assembly.FullName}': {ex.Message}");
                    return ex.Types.Where(t => t != null)!;
                }
            })
            .Where(type => typeof(T).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

        foreach (var type in derivedTypes)
        {
            if (type == null) continue;

            // 파생 클래스에서 어트리뷰트가 지정되지 않은 것은 건너뜁니다.
            var discriminatorValueAttr = type.GetCustomAttribute<JsonDiscriminatorValueAttribute>();
            if (discriminatorValueAttr == null) continue;

            // 중복 판별자 값이 있는 경우 예외를 발생시킵니다.
            if (typeMapping.ContainsKey(discriminatorValueAttr.PropertyValue))
                throw new JsonException($"Duplicate discriminator value '{discriminatorValueAttr.PropertyValue}'.");

            typeMapping[discriminatorValueAttr.PropertyValue] = type;
        }

        return typeMapping;
    }

}
