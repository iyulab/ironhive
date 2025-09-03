namespace IronHive.Abstractions.Collections;

/// <summary>
/// <see cref="IStorageCollection"/>에 저장될 수 있는 항목을 나타내는 마커 인터페이스입니다.
/// </summary>
public interface IStorageItem : IDisposable
{ }

/// <summary>
/// 문자열 키(<see cref="string"/>)와 <see cref="IStorageItem"/> 항목을 매핑하는 컬렉션입니다.
/// 다양한 종류의 저장소(Storage)들을 관리하는 데 사용됩니다.
/// </summary>
public interface IStorageCollection : IKeyedCollection<string, IStorageItem>
{ }
