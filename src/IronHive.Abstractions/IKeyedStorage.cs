namespace IronHive.Abstractions;

/// <summary>
/// 여러 스토리지 서비스를 구분하기 위한 인터페이스입니다.
/// </summary>
public interface IKeyedStorage : IDisposable
{
    /// <summary>
    /// 스토리지 서비스를 제공하는 공급자의 이름입니다. 식별자로 사용됩니다.
    /// </summary>
    string StorageName { get; }
}
