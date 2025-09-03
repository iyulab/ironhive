namespace IronHive.Plugins.MCP;

/// <summary>
/// MCP 서버의 연결 또는 연결해제 이벤트 인수입니다.
/// </summary>
public class McpConnectionEventArgs : EventArgs
{
    public McpConnectionEventArgs(string serverName)
    {
        ServerName = serverName;
        Timestamp = DateTimeOffset.Now;
    }

    /// <summary>
    /// 연결 서버의 이름입니다.
    /// </summary>
    public string ServerName { get; }

    /// <summary>
    /// 이벤트가 발생한 시각입니다.
    /// </summary>
    public DateTimeOffset Timestamp { get; }
}

/// <summary>
/// MCP 서버의 연결 에러 이벤트 인수입니다.
/// </summary>
public class McpErroredEventArgs : McpConnectionEventArgs
{
    public McpErroredEventArgs(string serverName, Exception? ex)
        : base(serverName)
    {
        Exception = ex;
    }

    /// <summary>
    /// 연결 중 발생한 예외입니다.
    /// </summary>
    public Exception? Exception { get; }
}