using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Net.Http.Json;
using IronHive.Abstractions.Json;

namespace System;

public static class ObjectExtensions
{
    /// <summary>
    /// 객체를 지정된 형식으로 변환을 시도합니다.
    /// 변환에 실패하면 null을 반환하며 false를 반환합니다.
    /// </summary>
    /// <typeparam name="T">변환 대상 형식</typeparam>
    /// <param name="obj">변환할 객체</param>
    /// <param name="value">변환된 결과 값 (실패 시 null)</param>
    /// <param name="options">JsonSerializer 옵션 (선택)</param>
    /// <returns>변환 성공 여부</returns>
    public static bool TryConvertTo<T>(
        this object? obj, 
        [MaybeNullWhen(false)] out T value, 
        JsonSerializerOptions? options = null)
    {
        value = obj.ConvertTo<T>(options);
        return value != null;
    }

    /// <summary>
    /// 객체를 지정된 형식으로 변환을 시도합니다.
    /// 변환에 실패하면 null을 반환하며 false를 반환합니다.
    /// </summary>
    /// <param name="obj">변환할 객체</param>
    /// <param name="target">변환 대상의 런타임 형식</param>
    /// <param name="value">변환된 결과 값 (실패 시 null)</param>
    /// <param name="options">JsonSerializer 옵션 (선택)</param>
    /// <returns>변환 성공 여부</returns>
    public static bool TryConvertTo(
        this object? obj, 
        Type target, 
        [MaybeNullWhen(false)] out object value, 
        JsonSerializerOptions? options = null)
    {
        value = obj.ConvertTo(target, options);
        return value != null;
    }

    /// <summary>
    /// 객체를 지정된 형식으로 변환합니다.
    /// 변환에 실패하면 기본값(default)을 반환합니다.
    /// </summary>
    /// <typeparam name="T">변환 대상 형식</typeparam>
    /// <param name="obj">변환할 객체</param>
    /// <param name="options">JsonSerializer 옵션 (선택)</param>
    /// <returns>변환된 객체 또는 기본값</returns>
    public static T? ConvertTo<T>(
        this object? obj, 
        JsonSerializerOptions? options = null)
    {
        return (T?)obj.ConvertTo(typeof(T), options);
    }

    /// <summary>
    /// 객체를 지정된 형식으로 변환합니다.
    /// 변환에 실패하면 기본값(default)을 반환합니다.
    /// </summary>
    /// <param name="obj">변환할 객체</param>
    /// <param name="target">변환 대상 형식</param>
    /// <param name="options">JsonSerializer 옵션 (선택)</param>
    /// <returns>변환된 객체 또는 null</returns>
    public static object? ConvertTo(
        this object? obj, 
        Type target, 
        JsonSerializerOptions? options = null)
    {
        try
        {
            options ??= JsonDefaultOptions.Options;
            if (obj == null)
            {
                // case 1: null인 경우
                return null;
            }
            else if (target.IsAssignableFrom(obj.GetType()))
            {
                // case 2: 이미 대상 형식인 경우
                return obj;
            }
            else if (obj is string str)
            {
                // case 3: 문자열일 경우
                return JsonSerializer.Deserialize(str, target, options);
            }
            else if (obj is JsonElement el)
            {
                // case 4: JsonElement인 경우
                return JsonSerializer.Deserialize(el.GetRawText(), target, options);
            }
            else if (obj is JsonDocument doc)
            {
                // case 5: JsonDocument인 경우
                return doc.Deserialize(target, options);
            }
            else if (obj is JsonNode node)
            {
                // case 6: JsonNode인 경우
                return node.Deserialize(target, options);
            }
            else if (obj is JsonContent jc)
            {
                // case 7: JsonContent인 경우
                var stream = jc.ReadAsStream();
                return JsonSerializer.Deserialize(stream, target, options);
            }
            else if (obj is JsonObject jo)
            {
                // case 8: JsonObject인 경우
                return jo.Deserialize(target, options);
            }
            else if (obj is JsonArray ja)
            {
                // case 9: JsonArray인 경우
                return ja.Deserialize(target, options);
            }
            else if (obj is JsonValue jv)
            {
                // case 10: JsonValue인 경우
                return jv.Deserialize(target, options);
            }
            else
            {
                // case 11: 기타 객체는 JSON으로 직렬화 후 역직렬화
                string jsonStr = JsonSerializer.Serialize(obj, options);
                return JsonSerializer.Deserialize(jsonStr, target, options);
            }
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 객체를 JSON 직렬화를 통해 깊은 복사(deep copy)합니다.
    /// 값 형식, 문자열 또는 null일 경우 원본을 그대로 반환합니다.
    /// </summary>
    /// <typeparam name="T">복사할 객체의 형식</typeparam>
    /// <param name="obj">복사할 객체</param>
    /// <returns>복사된 새 객체</returns>
    /// <exception cref="SerializationException">역직렬화에 실패한 경우 발생</exception>
    public static T Clone<T>(this T obj)
    {
        // null, 값 형식, 문자열이면 그대로 반환
        if (obj == null || obj.GetType().IsValueType || obj is string)
        {
            return obj;
        }

        var json = JsonSerializer.Serialize(obj, JsonDefaultOptions.CopyOptions);
        var clone = JsonSerializer.Deserialize<T>(json, JsonDefaultOptions.CopyOptions)
            ?? throw new SerializationException("Failed to deserialize the object after serialization.");
        return clone;
    }
}
