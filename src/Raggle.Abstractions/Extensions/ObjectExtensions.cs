using System.Runtime.Serialization;
using System.Text.Json;

namespace System;

public static class ObjectExtensions
{
    /// <summary>
    /// 객체를 깊은 복사합니다. 참조 타입은 깊은 복사가 이루어지고, 값 타입은 그대로 반환됩니다.
    /// </summary>
    public static T DeepCopy<T>(this T obj)
    {
        // 값 타입인 경우 그대로 반환
        if (obj == null || obj.GetType().IsValueType || obj is string)
        {
            return obj;
        }

        // JSON 직렬화를 통해 깊은 복사를 수행
        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            IncludeFields = true, // 필드도 직렬화
            PropertyNameCaseInsensitive = true
        };

        try
        {
            var json = JsonSerializer.Serialize(obj, options);
            var clone = JsonSerializer.Deserialize<T>(json, options)
                ?? throw new SerializationException("Failed to deserialize the object.");
            return clone;
        }
        catch (Exception ex)
        {
            throw new SerializationException("Failed to serialize the object, when deep copied", ex);
        }
    }
}
