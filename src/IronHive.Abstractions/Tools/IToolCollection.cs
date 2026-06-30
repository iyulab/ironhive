using System.Diagnostics.CodeAnalysis;

namespace IronHive.Abstractions.Tools;

/// <summary>
/// Hive 에이전트가 사용할 수 있는 도구(툴) 컬렉션을 관리합니다.
/// </summary>
public interface IToolCollection : ICollection<ITool>
{
    /// <summary>컬렉션에 존재하는 모든 키를 읽기 전용 컬렉션으로 반환합니다.</summary>
    IReadOnlyCollection<string> Keys { get; }

    /// <summary>지정한 키에 해당하는 도구를 안전하게 조회합니다.</summary>
    bool TryGet(string key, [MaybeNullWhen(false)] out ITool item);

    /// <summary>지정한 키가 컬렉션에 존재하는지 여부를 반환합니다.</summary>
    bool ContainsKey(string key);

    /// <summary>여러 도구를 컬렉션에 추가합니다. (키 중복 시 추가하지 않음)</summary>
    void AddRange(IEnumerable<ITool> items);

    /// <summary>도구를 추가하거나, 동일 키가 이미 존재하면 교체합니다.</summary>
#pragma warning disable CA1716
    void Set(ITool item);
#pragma warning restore CA1716

    /// <summary>여러 도구를 추가하거나 교체합니다.</summary>
    void SetRange(IEnumerable<ITool> items);

    /// <summary>지정한 키에 해당하는 도구를 제거합니다.</summary>
    bool Remove(string key);

    /// <summary>주어진 조건에 맞는 모든 도구를 제거합니다. null이면 전체 제거.</summary>
    int RemoveAll(Predicate<ITool>? match = null);

    /// <summary>
    /// 주어진 집합에 포함된 도구만 필터링하여 새로운 <see cref="IToolCollection"/>을 반환합니다.
    /// 존재하지 않는 키는 무시됩니다.
    /// </summary>
    IToolCollection FilterBy(IEnumerable<string> names);
}
