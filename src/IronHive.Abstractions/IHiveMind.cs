using IronHive.Abstractions.Agent;

namespace IronHive.Abstractions;

/// <summary>
/// HiveMind 인터페이스
/// </summary>
public interface IHiveMind
{
    /// <summary>
    /// HiveMind의 서비스 제공자
    /// </summary>
    IServiceProvider Services { get; }
}
