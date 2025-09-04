using System.Diagnostics.CodeAnalysis;

namespace IronHive.Abstractions;

/// <summary>
/// 키를 기반으로 항목을 관리할 수 있는 범용 컬렉션 인터페이스입니다.  
/// - 각 항목은 특정 키(<typeparamref name="TKey"/>)를 통해 접근할 수 있습니다.  
/// - 기존 <see cref="ICollection{T}"/> 기능에 키 기반 관리 기능을 확장합니다.  
/// </summary>
/// <typeparam name="TKey">항목을 식별하는 데 사용되는 키의 타입</typeparam>
/// <typeparam name="TItem">컬렉션에 저장되는 항목의 타입</typeparam>
public interface IKeyedCollection<TKey, TItem> : ICollection<TItem>
    where TKey : notnull
{
    /// <summary>
    /// 컬렉션에 존재하는 모든 키를 읽기 전용 컬렉션으로 반환합니다.  
    /// 순회, 존재 여부 확인 등의 용도로 활용할 수 있습니다.
    /// </summary>
    IReadOnlyCollection<TKey> Keys { get; }

    /// <summary>
    /// 지정한 키에 해당하는 항목을 안전하게 조회합니다.  
    /// - 항목이 존재하면 <paramref name="item"/>에 반환하고 <c>true</c>를 반환합니다.  
    /// - 없으면 <paramref name="item"/>은 기본값이 되고 <c>false</c>를 반환합니다.
    /// </summary>
    /// <param name="key">조회할 항목의 키</param>
    /// <param name="item">찾은 항목(없으면 기본값)</param>
    bool TryGet(TKey key, [MaybeNullWhen(false)] out TItem item);

    /// <summary>
    /// 지정한 키가 컬렉션에 존재하는지 여부를 반환합니다.  
    /// </summary>
    /// <param name="key">확인할 키</param>
    bool ContainsKey(TKey key);

    /// <summary>
    /// 항목을 컬렉션에 추가하거나, 동일 키가 이미 존재하면 해당 항목을 교체합니다.  
    /// 데이터의 최신 상태를 보장할 때 유용합니다.
    /// </summary>
    /// <param name="item">추가하거나 교체할 항목</param>
    void Set(TItem item);

    /// <summary>
    /// 지정한 키에 해당하는 항목을 제거합니다.  
    /// </summary>
    /// <param name="key">제거할 항목의 키</param>
    /// <returns>제거에 성공하면 <c>true</c>, 해당 키가 없으면 <c>false</c></returns>
    bool Remove(TKey key);

    /// <summary>
    /// 주어진 조건에 맞는 모든 항목을 제거합니다.  
    /// 조건이 <c>null</c>이면 컬렉션의 모든 항목을 제거합니다.  
    /// </summary>
    /// <param name="match">제거할 항목을 판별하는 조건식. 없으면 전체 제거.</param>
    /// <returns>제거된 항목의 개수</returns>
    int RemoveAll(Predicate<TItem>? match = null);
}
