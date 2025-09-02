namespace IronHive.Abstractions;

/// <summary>
/// 여러 파생 타입(<typeparamref name="TDerived"/>)을
/// <b>타입별 그룹</b>으로 관리하는 키 기반 컬렉션입니다.
/// 
/// <para>
/// 공통 부모 타입 <typeparamref name="TBase"/>를 기반으로, 하위 타입별로
/// 독립된 KeyedCollection을 유지하여 타입 안전한 조회/추가/제거를 지원합니다.
/// </para>
/// </summary>
/// <typeparam name="TBase">저장할 항목의 공통 부모 타입</typeparam>
public interface IKeyedCollectionGroup<TBase> : IEnumerable<TBase>
    where TBase : class
{
    /// <summary>
    /// 현재 그룹에 존재하는 모든 파생 타입 컬렉션의 타입 목록을 반환합니다.
    /// </summary>
    IReadOnlyCollection<Type> Types { get; }

    /// <summary>
    /// 지정된 타입의 키 컬렉션을 반환합니다.
    /// <para>존재하지 않으면 생성(지연 생성)되며, 라이브 뷰를 반환합니다(스냅샷 아님).</para>
    /// </summary>
    IKeyedCollection<TDerived> Of<TDerived>() where TDerived : class, TBase;

    /// <summary>
    /// 지정한 타입 그룹의 모든 항목 수를 반환합니다.
    /// </summary>
    int Count<TDerived>() where TDerived : class, TBase;

    /// <summary>
    /// 지정한 타입 그룹에 항목이 하나라도 존재하는지 확인합니다.
    /// </summary>
    bool Any<TDerived>() where TDerived : class, TBase;

    /// <summary>
    /// 지정된 타입 그룹의 모든 키를 반환합니다.
    /// </summary>
    IReadOnlyCollection<string> GetKeys<TDerived>() where TDerived : class, TBase;

    /// <summary>
    /// 지정된 타입 그룹의 모든 항목을 반환합니다.
    /// </summary>
    IReadOnlyCollection<TDerived> GetValues<TDerived>() where TDerived : class, TBase;

    /// <summary>
    /// 지정된 키의 항목을 반환합니다. 없으면 <c>null</c>.
    /// </summary>
    TDerived? Get<TDerived>(string key) where TDerived : class, TBase;

    /// <summary>
    /// 지정된 키의 항목을 시도하여 가져옵니다.
    /// </summary>
    bool TryGet<TDerived>(string key, out TDerived item) where TDerived : class, TBase;

    /// <summary>
    /// 컬렉션에 지정된 아이템이 존재하는지 확인합니다.
    /// </summary>
    bool Contains<TDerived>(TDerived item) where TDerived : class, TBase;

    /// <summary>
    /// 컬렉션에 지정된 키가 존재하는지 확인합니다.
    /// </summary>
    bool ContainsKey<TDerived>(string key) where TDerived : class, TBase;

    /// <summary>
    /// 항목을 추가합니다. 같은 키가 있으면 예외 발생.
    /// </summary>
    void Add<TDerived>(TDerived item) where TDerived : class, TBase;

    /// <summary>
    /// 항목을 설정합니다. 같은 키가 있으면 덮어씁니다.
    /// </summary>
    void Set<TDerived>(TDerived item) where TDerived : class, TBase;

    /// <summary>
    /// 지정된 키의 항목을 제거합니다.
    /// </summary>
    bool Remove<TDerived>(string key) where TDerived : class, TBase;

    /// <summary>
    /// 조건에 맞는 모든 항목을 제거하고, 제거된 개수를 반환합니다.
    /// </summary>
    int RemoveAll<TDerived>(Predicate<TDerived> match) where TDerived : class, TBase;

    /// <summary>
    /// 해당 타입 그룹의 모든 항목을 제거합니다.
    /// </summary>
    void Clear<TDerived>() where TDerived : class, TBase;
}
