using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace IronHive.Abstractions.Tools;

/// <summary>
/// 도구 실행 시 전달되는 입력 데이터를 래핑한 타입입니다.
/// 내부적으로 키-값 쌍으로 구성되며, 읽기 전용 딕셔너리로 사용됩니다.
/// </summary>
public class ToolInput : IReadOnlyDictionary<string, object?>
{
    private readonly Dictionary<string, object?> _items;

    /// <summary>
    /// 주어진 입력 객체를 딕셔너리 형태로 변환하여 래핑합니다.
    /// </summary>
    public ToolInput(object? input = null, object? options = null, IServiceProvider? services = null)
    {
        _items = input is null
                 ? new Dictionary<string, object?>(StringComparer.Ordinal)
                 : input.ConvertTo<Dictionary<string, object?>>()
                 ?? throw new ArgumentException("입력 객체는 Dictionary<string, object?> 형식이어야 합니다.", nameof(input));

        Services = services;
        Options = options;
    }

    /// <summary>
    /// 툴에 전달되는 추가 옵션입니다.
    /// </summary>
    public object? Options { get; set; }

    /// <summary>
    /// 툴 실행에 필요한 서비스 제공자입니다.
    /// </summary>
    public IServiceProvider? Services { get; set; }

    /// <summary>
    /// 타입 변환을 시도하여 지정된 형식의 값을 반환합니다.
    /// </summary>
    public bool TryGetValue<T>(string key, [MaybeNullWhen(false)] out T value)
    {
        if (_items.TryGetValue(key, out var obj) && obj.TryConvertTo<T>(out var t))
        {
            value = t;
            return true;
        }

        value = default;
        return false;
    }

    /// <inheritdoc />
    public object? this[string key]
    {
        get => _items.TryGetValue(key, out var value) ? value : null;
    }

    /// <inheritdoc />
    public IEnumerable<string> Keys => _items.Keys;

    /// <inheritdoc />
    public IEnumerable<object?> Values => _items.Values;

    /// <inheritdoc />
    public int Count => _items.Count;

    /// <inheritdoc />
    public bool ContainsKey(string key) => _items.ContainsKey(key);

    /// <inheritdoc />
    public bool TryGetValue(string key, out object? value) => _items.TryGetValue(key, out value);
    
    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _items.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

    /// <inheritdoc />
    public override string? ToString() => JsonSerializer.Serialize(_items);
}
