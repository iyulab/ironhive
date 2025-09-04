using IronHive.Abstractions.Files;
using IronHive.Abstractions.Queue;
using IronHive.Abstractions.Vector;

namespace IronHive.Abstractions.Registries;

/// <summary>
/// <see cref="IStorageRegistry"/>에 등록될 수 있는 항목을 나타내는 마커 인터페이스입니다.
/// - 모든 스토리지 항목은 이 인터페이스를 구현해야 하며,  
/// - 리소스 해제를 위해 <see cref="IDisposable"/>을 상속받습니다.
/// </summary>
public interface IStorageItem : IDisposable
{ }

/// <summary>
/// 문자열 키(<see cref="string"/>)와 <see cref="IStorageItem"/>을 매핑하는 스토리지 레지스트리입니다.  
/// - 파일, 큐, 벡터 등 다양한 저장소 타입을 통합 관리합니다.
/// </summary>
public interface IStorageRegistry : IKeyedServiceRegistry<string, IStorageItem>
{
    /// <summary>
    /// 파일 스토리지를 등록합니다.
    /// </summary>
    /// <param name="storageName">스토리지의 고유 이름</param>
    /// <param name="storage">파일 스토리지 인스턴스</param>
    void SetFileStorage(string storageName, IFileStorage storage);

    /// <summary>
    /// 큐 스토리지를 등록합니다.
    /// </summary>
    /// <param name="storageName">스토리지의 고유 이름</param>
    /// <param name="storage">큐 스토리지 인스턴스</param>
    void SetQueueStorage(string storageName, IQueueStorage storage);

    /// <summary>
    /// 벡터 스토리지를 등록합니다.
    /// </summary>
    /// <param name="storageName">스토리지의 고유 이름</param>
    /// <param name="storage">벡터 스토리지 인스턴스</param>
    void SetVectorStorage(string storageName, IVectorStorage storage);
}
