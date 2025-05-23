namespace IronHive.Abstractions.Tools;

/// <summary>
/// 툴 플러그인을 관리하는 매니저 인터페이스입니다.
/// 등록된 툴 플러그인을 통해 도구 목록 조회 및 실행 기능을 제공합니다.
/// </summary>
public interface IToolPluginManager
{
    /// <summary>
    /// 로드된 모든 툴 플러그인을 이름(Key)과 함께 보관하는 딕셔너리입니다.
    /// </summary>
    IDictionary<string, IToolPlugin> Plugins { get; }

    /// <summary>
    /// 모든 등록된 플러그인에서 사용할 수 있는 툴 목록을 비동기적으로 가져옵니다.
    /// </summary>
    public Task<IEnumerable<ToolDescriptor>> ListAsync(
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
    public Task<ToolOutput> InvokeAsync(
        string name,
        ToolInput input,
        CancellationToken cancellationToken = default);
}
