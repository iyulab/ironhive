using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace IronHive.Abstractions.Tools;

/// <summary>
/// 도구 실행 시 전달되는 입력 데이터를 래핑한 타입입니다.
/// 내부적으로 키-값 쌍으로 구성되며, 읽기 전용 딕셔너리로 사용됩니다.
/// </summary>
public class ToolInput : IReadOnlyDictionary<string, object?>
{
    private readonly Dictionary<string, object?> _inner;

    /// <summary>
    /// 빈 입력을 생성합니다.
    /// </summary>
    public ToolInput()
    {
        _inner = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 주어진 입력 객체를 딕셔너리 형태로 변환하여 래핑합니다.
    /// </summary>
    /// <param name="input">입력 객체 (보통 익명 객체 또는 Dictionary)</param>
    public ToolInput(object? input)
    {
        _inner = input?.ConvertTo<Dictionary<string, object?>>()
                 ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 키에 해당하는 값을 가져옵니다. 존재하지 않으면 null 반환.
    /// </summary>
    public object? this[string key]
    {
        get => _inner.TryGetValue(key, out var value) ? value : null;
    }

    public IEnumerable<string> Keys => _inner.Keys;

    public IEnumerable<object?> Values => _inner.Values;

    public int Count => _inner.Count;

    public bool ContainsKey(string key) => _inner.ContainsKey(key);

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value)
        => _inner.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        => _inner.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => _inner.GetEnumerator();
}
