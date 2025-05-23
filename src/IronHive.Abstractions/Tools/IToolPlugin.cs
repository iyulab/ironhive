namespace IronHive.Abstractions.Tools;

/// <summary>
/// 도구(툴)를 정의하고 실행할 수 있는 플러그인 인터페이스입니다.
/// 이 인터페이스는 툴의 메타데이터를 나열하고, 특정 툴을 실행하는 기능을 제공합니다.
/// </summary>
public interface IToolPlugin : IDisposable
{
    /// <summary>
    /// 플러그인의 고유 이름입니다.
    /// 툴 플러그인을 식별하는 데 사용됩니다.
    /// </summary>
    string PluginName { get; }

    /// <summary>
    /// 이 플러그인에서 사용할 수 있는 모든 도구의 메타데이터를 비동기적으로 가져옵니다.
    /// </summary>
    /// <returns>
    /// 비동기적으로 사용할 수 있는 <see cref="ToolDescriptor"/> 객체의 컬렉션을 반환합니다.
    /// </returns>
    Task<IEnumerable<ToolDescriptor>> ListAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 이름의 도구를 실행합니다.
    /// </summary>
    /// <param name="name">실행할 도구의 이름입니다.</param>
    /// <param name="input">도구에 전달할 입력 데이터입니다. 일반적으로 JSON 기반 객체입니다.</param>
    /// <returns>
    /// 비동기적으로 도구의 실행 결과를 나타내는 <see cref="ToolOutput"/> 객체를 반환합니다.
    /// </returns>
    Task<ToolOutput> InvokeAsync(
        string name,
        ToolInput input,
        CancellationToken cancellationToken = default);
}
