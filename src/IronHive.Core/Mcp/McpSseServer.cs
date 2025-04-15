namespace IronHive.Core.Mcp;

/// <summary>
/// SSE(Server-Sent Events) 방식의 MCP 서버를 나타냅니다.
/// </summary>
public class McpSseServer : McpServerBase
{
    /// <summary>
    /// 서버의 엔드포인트 URL을 가져오거나 설정합니다.
    /// </summary>
    public required Uri Endpoint { get; set; }

    /// <summary>
    /// 추가 헤더를 가져오거나 설정합니다.
    /// </summary>
    public Dictionary<string, string>? AdditionalHeaders { get; set; }

    /// <summary>
    /// 서버에 연결할 때의 타임아웃 시간을 설정합니다.
    /// </summary>
    public TimeSpan ConnectionTimeout { get; set; }

    /// <summary>
    /// 서버에 재연결 시 대기할 시간을 설정합니다.
    /// </summary>
    public TimeSpan ReconnectDelay { get; set; }

    /// <summary>
    /// 최대 재연결 시도 횟수를 설정합니다.
    /// </summary>
    public int MaxReconnectAttempts { get; set; }
}
