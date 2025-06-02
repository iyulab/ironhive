using System.Diagnostics.CodeAnalysis;

namespace IronHive.Abstractions.Tools;

/// <summary>
/// 툴 플러그인을 관리하는 매니저 인터페이스입니다.
/// 등록된 툴 플러그인을 통해 도구 목록 조회 및 실행 기능을 제공합니다.
/// </summary>
public interface IToolPluginManager
{
    /// <summary>
    /// 지정한 이름의 플러그인이 매니저에 등록되어 있는지 여부를 반환합니다.
    /// </summary>
    bool ContainsPlugin(string name);

    /// <summary>
    /// 새로운 플러그인을 매니저에 등록합니다.
    /// 플러그인 등록에 성공하면 true, 이미 동일한 이름의 플러그인이 존재하거나 등록에 실패하면 false를 반환합니다.
    /// </summary>
    bool TryAddPlugin(IToolPlugin plugin);

    /// <summary>
    /// 지정한 이름의 플러그인을 매니저에서 제거합니다.
    /// </summary>
    bool TryRemovePlugin(string name);

    /// <summary>
    /// 모든 등록된 플러그인에서 사용할 수 있는 툴 목록을 비동기적으로 가져옵니다.
    /// </summary>
    Task<IEnumerable<ToolDescriptor>> ListAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정한 이름의 툴을 실행합니다.
    /// </summary>
    /// <param name="name">실행할 툴의 이름입니다.</param>
    /// <param name="input">툴에 전달할 입력 데이터입니다. 일반적으로 JSON 객체 형식입니다.</param>
    /// <param name="cancellationToken">작업 취소를 위한 토큰입니다.</param>
    /// <returns>
    /// 비동기적으로 툴의 실행 결과를 나타내는 <see cref="ToolOutput"/> 객체를 반환합니다.
    /// </returns>
    Task<ToolOutput> InvokeAsync(
        string name,
        ToolInput input,
        CancellationToken cancellationToken = default);
}
