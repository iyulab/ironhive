namespace IronHive.Core.Mcp;

/// <summary>
/// 표준 입력/출력 방식의 MCP 서버를 나타냅니다.
/// </summary>
public class McpStdioServer : McpServerBase
{
    /// <summary>
    /// 서버에서 실행할 명령을 가져오거나 설정합니다.
    /// </summary>
    public required string Command { get; set; }

    /// <summary>
    /// 명령에 전달할 인수를 가져오거나 설정합니다.
    /// </summary>
    public IEnumerable<string>? Arguments { get; set; }

    /// <summary>
    /// 서버에서 사용하는 환경 변수를 가져오거나 설정합니다.
    /// </summary>
    public Dictionary<string, string>? EnvironmentVariables { get; set; }

    /// <summary>
    /// 서버가 종료될 때까지 기다리는 최대 시간을 설정합니다.
    /// </summary>
    public TimeSpan ShutdownTimeout { get; set; }

    /// <summary>
    /// 서버가 실행될 작업 디렉터리를 가져오거나 설정합니다.
    /// </summary>
    public string? WorkingDirectory { get; set; }
}
