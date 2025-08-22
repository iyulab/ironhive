namespace IronHive.Plugins.MCP;

/// <summary>
/// MCP 서버의 연결 이벤트 인수입니다.
/// </summary>
public class McpConnectedEventArgs : EventArgs
{
    public McpConnectedEventArgs(string serverName)
    {
        ServerName = serverName;
        Timestamp = DateTimeOffset.Now;
    }

    /// <summary>
    /// 연결 서버의 이름입니다.
    /// </summary>
    public string ServerName { get; }

    /// <summary>
    /// 연결 해제 시간
    /// </summary>
    public DateTimeOffset Timestamp { get; }
}

/// <summary>
/// MCP 서버의 연결 해제 이벤트 인수입니다.
/// </summary>
public class McpDisconnectedEventArgs : EventArgs
{
    public McpDisconnectedEventArgs(string serverName)
    {
        ServerName = serverName;
        Timestamp = DateTimeOffset.Now;
    }

    /// <summary>
    /// 연결 서버의 이름입니다.
    /// </summary>
    public string ServerName { get; }

    /// <summary>
    /// 연결 해제 시간
    /// </summary>
    public DateTimeOffset Timestamp { get; }
}

/// <summary>
/// MCP 서버의 연결 에러 이벤트 인수입니다.
/// </summary>
public class McpErroredEventArgs : EventArgs
{
    public McpErroredEventArgs(string serverName, Exception? ex)
    {
        ServerName = serverName;
        Exception = ex;
        Timestamp = DateTimeOffset.Now;
    }

    /// <summary>
    /// 연결 서버의 이름입니다.
    /// </summary>
    public string ServerName { get; }

    /// <summary>
    /// 연결 중 발생한 예외입니다.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// 연결 해제 시간
    /// </summary>
    public DateTimeOffset Timestamp { get; }
}