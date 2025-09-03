using System.Diagnostics.CodeAnalysis;

namespace IronHive.Abstractions.Collections;

/// <summary>
/// 제네릭 기반의 “타입별 카테고리 + 문자열 키” 조합으로 객체를 저장/조회/삭제하는 저장소 계약입니다.
/// 각 메서드는 <c>TCategory</c> 타입 파라미터로 “카테고리(버킷)”를 지정합니다.
/// 즉, 동일한 키라도 카테고리가 다르면 서로 다른 공간에 저장됩니다.
/// </summary>
/// <typeparam name="TKey">저장 가능한 항목들의 키 타입.</typeparam>
/// <typeparam name="TBase">저장 가능한 항목들의 공통 상위 타입(기준 타입).</typeparam>
public interface IKeyedCollection<TKey, TBase>
    where TKey : notnull
    where TBase : class
{
    /// <summary>
    /// 지정한 카테고리(<typeparamref name="TCategory"/>) 버킷에 들어있는 항목 수를 반환합니다.
    /// </summary>
    int Count<TCategory>()
        where TCategory : class, TBase;

    /// <summary>
    /// 지정한 키가 카테고리(<typeparamref name="TCategory"/>) 버킷에 존재하는지 확인합니다.
    /// </summary>
    /// <param name="key">조회할 항목의 문자열 키.</param>
    /// <returns>키 존재 여부.</returns>
    bool Contains<TCategory>(TKey key)
        where TCategory : class, TBase;

    /// <summary>
    /// 지정한 키에 해당하는 항목을 시도하여 가져옵니다.
    /// </summary>
    /// <param name="key">가져올 항목의 문자열 키.</param>
    /// <param name="item">성공 시 항목 인스턴스, 실패 시 <c>null</c>.</param>
    /// <returns>가져오기에 성공하면 <c>true</c>, 아니면 <c>false</c>.</returns>
    bool TryGet<TCategory>(TKey key, [MaybeNullWhen(false)] out TCategory item)
        where TCategory : class, TBase;

    /// <summary>
    /// 항목을 추가합니다. 동일 키가 이미 존재하면 실패합니다.
    /// </summary>
    /// <typeparam name="TItem">
    /// 실제 저장할 항목의 구체 타입. <typeparamref name="TCategory"/>를 상속해야 합니다.
    /// </typeparam>
    /// <param name="key">추가할 항목의 문자열 키.</param>
    /// <param name="item">추가할 인스턴스.</param>
    /// <returns>성공적으로 추가되면 <c>true</c>, 중복 등으로 실패하면 <c>false</c>.</returns>
    bool TryAdd<TCategory, TItem>(TKey key, TItem item)
        where TCategory : class, TBase
        where TItem : class, TCategory;

    /// <summary>
    /// 항목을 설정합니다. 동일 키가 있으면 바꿔치기(upsert)합니다.
    /// </summary>
    /// <typeparam name="TItem">
    /// 실제 저장할 항목의 구체 타입. <typeparamref name="TCategory"/>를 상속해야 합니다.
    /// </typeparam>
    /// <param name="key">설정할 항목의 문자열 키.</param>
    /// <param name="item">설정할 인스턴스.</param>
    void Set<TCategory, TItem>(TKey key, TItem item)
        where TCategory : class, TBase
        where TItem : class, TCategory;

    /// <summary>
    /// 지정한 키에 해당하는 항목을 삭제합니다.
    /// </summary>
    /// <param name="key">삭제할 항목의 문자열 키.</param>
    /// <returns>삭제 성공 여부.</returns>
    bool Remove<TCategory>(TKey key)
        where TCategory : class, TBase;

    /// <summary>
    /// 지정한 조건에 일치하는 모든 항목을 찾아 삭제합니다.
    /// </summary>
    /// <param name="match">삭제 기준이 되는 조건자(Predicate). null이면 모든 항목을 삭제합니다.</param>
    /// <returns>삭제된 항목 수.</returns>
    int RemoveAll<TCategory>(Predicate<TCategory>? match = null)
        where TCategory : class, TBase;
}








