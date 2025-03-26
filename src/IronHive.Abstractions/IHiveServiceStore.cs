using System.Diagnostics.CodeAnalysis;

namespace IronHive.Abstractions;

/// <summary>
/// Key-Instance 쌍으로 서비스를 저장 및 관리하는 인터페이스입니다.
/// </summary>
public interface IHiveServiceStore
{
    /// <summary>
    /// 등록된 모든 서비스 인스턴스를 반환합니다.
    /// </summary>
    IReadOnlyDictionary<string, TService> GetServices<TService>();

    /// <summary>
    /// 지정한 키에 해당하는 서비스 인스턴스를 가져오려고 시도합니다.
    /// 없으면 false를 반환합니다.
    /// </summary>
    bool TryGetService<TService>(string key, [MaybeNullWhen(false)] out TService instance);

    /// <summary>
    /// 지정한 키에 해당하는 서비스 인스턴스를 반환합니다.
    /// 없으면 예외를 발생시킵니다.
    /// </summary>
    TService GetService<TService>(string key);

    /// <summary>
    /// 지정한 키로 서비스 인스턴스를 등록하려고 시도합니다.
    /// 이미 등록되어 있으면 false를 반환합니다.
    /// </summary>
    bool TryAddService<TService>(string key, TService instance);

    /// <summary>
    /// 지정한 키로 서비스 인스턴스를 등록합니다.
    /// </summary>
    void AddService<TService>(string key, TService instance);

    /// <summary>
    /// 지정한 키에 해당하는 서비스 인스턴스를 제거하려고 시도합니다.
    /// </summary>
    bool TryRemoveService<TService>(string key);

    /// <summary>
    /// 지정한 키에 해당하는 서비스 인스턴스를 제거합니다.
    /// </summary>
    void RemoveService<TService>(string key);

    /// <summary>
    /// 지정한 키로 등록된 서비스가 존재하는지 확인합니다.
    /// </summary>
    bool ContainsService<TService>(string key);
}
