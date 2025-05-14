using IronHive.Abstractions.Tools;

namespace IronHive.Core.Mcp;

/// <summary>
/// MCP 서버 관리 인터페이스
/// </summary>
public interface IMcpServerManager
{
    /// <summary>
    /// 지정된 서버 ID가 실행 중인지 여부를 비동기적으로 확인합니다.
    /// </summary>
    /// <param name="id">서버의 고유 식별자</param>
    /// <returns>서버가 실행 중이면 true, 아니면 false</returns>
    Task<bool> IsRunningAsync(
        string id, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 서버를 시작합니다.
    /// </summary>
    /// <param name="id">서버의 고유 식별자</param>
    /// <param name="server">시작할 서버 인스턴스</param>
    Task StartAsync(
        string id, 
        IMcpServer server, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 서버를 중지합니다.
    /// </summary>
    /// <param name="id">서버의 고유 식별자</param>
    Task StopAsync(
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 서버의 툴을 가져옵니다.
    /// </summary>
    Task<IEnumerable<ITool>> ListToolsAsync(
            string id,
            CancellationToken cancellationToken = default);
}
