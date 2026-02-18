using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Json;

/// <summary>
/// JSON String의 다형성을 지원하는 컨버터입니다.
/// </summary>
public class PolymorphicJsonConverter<T> : JsonConverter<T> where T : class
{
    private const string DefaultDiscriminatorName = "type";
    private const StringComparison DefaultDiscriminatorComparison = StringComparison.OrdinalIgnoreCase;

    private readonly string _discriminatorName;
    private readonly StringComparison _discriminatorComparison;

    private static readonly Lazy<Dictionary<string, Type>> _cachedTypeMapper = new(() => GetTypeMapper());
    private readonly Dictionary<string, Type> _typeMapper;

    /// <summary>
    /// 사용자 지정 판별자 이름을 사용하는 생성자입니다.
    /// </summary>
    public PolymorphicJsonConverter()
    {
        var attr = typeof(T).GetCustomAttribute<JsonPolymorphicNameAttribute>();
        _discriminatorName = attr?.Name ?? DefaultDiscriminatorName;
        _discriminatorComparison = DefaultDiscriminatorComparison;
        _typeMapper = _cachedTypeMapper.Value;
    }

    /// <summary>
    /// 사용자 지정 판별자 이름 및 타입 매핑을 사용하는 생성자입니다.
    /// </summary>
    /// <param name="discriminatorName">JSON 타입 판별자 속성 이름</param>
    /// <param name="discriminatorComparison">판별자 이름의 비교 방법</param>
    /// <param name="typeMapping">파생 클래스와 판별자 값의 매핑</param>
    public PolymorphicJsonConverter(
        string? discriminatorName = null,
        StringComparison? discriminatorComparison = null,
        Dictionary<string, Type>? typeMapping = null)
    {
        _discriminatorName = discriminatorName ?? DefaultDiscriminatorName;
        _discriminatorComparison = discriminatorComparison ?? DefaultDiscriminatorComparison;
        _typeMapper = typeMapping ?? _cachedTypeMapper.Value;
    }

    // Deserialize Method
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Load the JSON document
        using (var jsonDoc = JsonDocument.ParseValue(ref reader))
        {
            var discriminatorProperty = jsonDoc.RootElement.EnumerateObject()
                .FirstOrDefault(prop => prop.Name.Equals(_discriminatorName, _discriminatorComparison));

            if (discriminatorProperty.Equals(default))
                throw new JsonException($"Missing discriminator property '{_discriminatorName}'.");

            var discriminatorValue = discriminatorProperty.Value.GetString();
            if (string.IsNullOrEmpty(discriminatorValue) || !_typeMapper.TryGetValue(discriminatorValue, out var targetType))
                throw new JsonException($"Unknown discriminator value '{discriminatorValue}', Are you missing a type mapping?");

            var jsonObject = jsonDoc.RootElement.GetRawText();
            var instance = JsonSerializer.Deserialize(jsonObject, targetType, options)
                ?? throw new JsonException($"Failed to deserialize JSON to '{targetType.Name}'.");

            return (T)instance;
        }
    }

    // Serialize Method
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var type = value.GetType();
        var discriminatorValue = _typeMapper.FirstOrDefault(x => x.Value == type).Key;

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

    /// <summary>
    /// 현재 어셈블리에서 T의 파생 클래스를 검색하고, 식별자 값과 타입을 매핑합니다.
    /// </summary>
    /// <returns>식별자 값과 타입의 매핑 딕셔너리</returns>
    private static Dictionary<string, Type> GetTypeMapper()
    {
        var typeMapper = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
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
                    Trace.TraceWarning($"Error loading types from assembly '{assembly.FullName}': {ex.Message}");
                    return ex.Types.Where(t => t != null)!;
                }
            })
            .Where(type => typeof(T).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

        foreach (var type in derivedTypes)
        {
            if (type == null) continue;

            // 파생 클래스에서 어트리뷰트가 지정되지 않은 것은 건너뜁니다.
            var discriminatorValueAttr = type.GetCustomAttribute<JsonPolymorphicValueAttribute>();
            if (discriminatorValueAttr == null) continue;

            // 중복 판별자 값이 있는 경우 예외를 발생시킵니다.
            if (!typeMapper.TryAdd(discriminatorValueAttr.Value, type))
                throw new JsonException($"Duplicate discriminator value '{discriminatorValueAttr.Value}'.");
        }

        return typeMapper;
    }
}
