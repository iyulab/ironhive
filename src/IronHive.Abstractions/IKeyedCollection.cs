namespace IronHive.Abstractions;

/// <summary>
/// 문자열 키를 기반으로 아이템을 관리하는 컬렉션을 정의합니다.
/// </summary>
public interface KeyedCollection<T> : ICollection<T>
{
    /// <summary>
    /// 모든 키를 반환합니다.
    /// </summary>
    IReadOnlyCollection<string> Keys { get; }

    /// <summary>
    /// 모든 아이템을 반환합니다.
    /// </summary>
    IReadOnlyCollection<T> Values { get; }

    /// <summary>
    /// 지정된 키에 해당하는 아이템을 반환합니다.
    /// 해당하는 아이템이 없으면 <c>null</c>을 반환합니다.
    /// </summary>
    /// <param name="key">찾고자 하는 아이템의 고유 문자열 키.</param>
    /// <returns>해당 키에 매핑된 아이템, 없으면 <c>null</c>.</returns>
    T? Get(string key);

    /// <summary>
    /// 지정된 키에 해당하는 아이템을 안전하게 검색합니다.
    /// </summary>
    /// <param name="key">찾고자 하는 아이템의 고유 문자열 키.</param>
    /// <param name="item">해당 키에 매핑된 아이템(없으면 <c>null</c>).</param>
    /// <returns>아이템이 존재하면 <c>true</c>, 없으면 <c>false</c>.</returns>
    bool TryGet(string key, out T item);

    /// <summary>
    /// 지정된 키가 컬렉션에 존재하는지 여부를 반환합니다.
    /// </summary>
    /// <param name="key">검사할 키.</param>
    bool ContainsKey(string key);

    /// <summary>
    /// 아이템의 키(구현체의 key selector로 계산)가 이미 존재하면 교체하고,
    /// 존재하지 않으면 추가합니다.
    /// </summary>
    /// <param name="item">추가 또는 교체할 아이템.</param>
    void Set(T item);

    /// <summary>
    /// 지정된 키의 아이템을 제거합니다.
    /// 아이템 존재 여부를 반환합니다.
    /// </summary>
    /// <param name="key">제거할 아이템의 키.</param>
    /// <returns>아이템이 존재하여 제거되면 <c>true</c>, 없으면 <c>false</c>.</returns>
    bool Remove(string key);

    /// <summary>
    /// 일치하는 모든 아이템을 제거합니다.
    /// </summary>
    /// <param name="match">제거할 아이템을 결정하는 조건.</param>
    /// <returns>제거된 아이템의 수.</returns>
    int RemoveAll(Predicate<T> match);

    /// <summary>
    /// 컬렉션에 등록된 모든 아이템과 그 키를 읽기 전용의 딕셔너리로 반환합니다.
    /// </summary>
    IReadOnlyDictionary<string, T> ToDictionary();
}
