namespace IronHive.Plugins.MCP;

/// <summary>
/// MCP 서버의 연결 이벤트 인수입니다.
/// </summary>
public class McpConnectedEventArgs : EventArgs
{
    public McpConnectedEventArgs(string pluginName)
    {
        PluginName = pluginName;
        Timestamp = DateTimeOffset.Now;
    }

    /// <summary>
    /// 연결 플러그인의 이름입니다.
    /// </summary>
    public string PluginName { get; }

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
    public McpDisconnectedEventArgs(string pluginName, Exception? exception = null)
    {
        PluginName = pluginName;
        Exception = exception;
        Timestamp = DateTimeOffset.Now;
    }

    /// <summary>
    /// 연결 플러그인의 이름입니다.
    /// </summary>
    public string PluginName { get; }

    /// <summary>
    /// 연결 해제의 원인이 된 오류 (정상 해제인 경우 null)
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// 연결 해제 시간
    /// </summary>
    public DateTimeOffset Timestamp { get; }
}
