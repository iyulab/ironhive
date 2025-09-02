namespace IronHive.Abstractions;

/// <summary>
/// 여러 서비스 제공자(Provider)를 구분하기 위한 인터페이스입니다.
/// </summary>
public interface IKeyedProvider : IDisposable
{
    /// <summary>
    /// 서비스를 제공하는 공급자의 이름입니다. 식별자로 사용됩니다.
    /// </summary>
    string ProviderName { get; }
}
