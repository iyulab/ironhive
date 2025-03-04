namespace Raggle.Abstractions;

/// <summary>
/// 키를 사용하여 서비스를 등록하고 조회할 수 있는 인터페이스를 정의합니다.
/// </summary>
public interface IHiveServiceContainer
{
    /// <summary>
    /// 지정된 서비스 타입에 등록된 모든 키와 인스턴스의 딕셔너리를 반환합니다.
    /// </summary>
    IReadOnlyDictionary<string, TService> GetKeyedServices<TService>()
        where TService : class;

    /// <summary>
    /// 지정된 키에 해당하는 서비스 인스턴스를 반환합니다.
    /// </summary>
    TService GetKeyedService<TService>(string serviceKey)
        where TService : class;

    /// <summary>
    /// 지정된 키와 함께 서비스 인스턴스를 등록합니다.
    /// </summary>
    void RegisterKeyedService<TService, TImplementation>(string serviceKey, TImplementation instance)
        where TService : class
        where TImplementation : class, TService;

    /// <summary>
    /// 지정된 키에 해당하는 서비스를 등록 해제합니다.
    /// </summary>
    void UnregisterKeyedService<TService>(string serviceKey)
        where TService : class;

    /// <summary>
    /// 지정된 키에 해당하는 서비스가 등록되어 있는지 확인합니다.
    /// </summary>
    bool ContainsKeyedService<TService>(string serviceKey)
        where TService : class;
}
