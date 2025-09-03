namespace IronHive.Abstractions.Collections;

/// <summary>
/// <see cref="IProviderCollection"/>에 저장될 수 있는 항목을 나타내는 마커 인터페이스입니다.
/// </summary>
public interface IProviderItem : IDisposable
{ }

/// <summary>
/// 문자열 키(<see cref="string"/>)와 <see cref="IProviderItem"/> 항목을 매핑하는 컬렉션입니다.
/// AI 모델을 제공하는 서비스 제공자(Provider)들을 관리하는 데 사용됩니다.
/// </summary>
public interface IProviderCollection : IKeyedCollection<string, IProviderItem>
{ }
