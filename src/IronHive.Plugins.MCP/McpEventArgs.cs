namespace IronHive.Plugins.MCP;

/// <summary>
/// 연결 이벤트 인수입니다.
/// </summary>
public class ConnectedEventArgs : EventArgs
{
    public ConnectedEventArgs(string serverName)
    {
        Name = serverName;
        Timestamp = DateTimeOffset.Now;
    }

    /// <summary>
    /// 연결 플러그인의 이름입니다.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 연결 해제 시간
    /// </summary>
    public DateTimeOffset Timestamp { get; }
}

/// <summary>
/// 연결 해제 이벤트 인수입니다.
/// </summary>
public class DisconnectedEventArgs : EventArgs
{
    public DisconnectedEventArgs(string serverName, Exception? error = null)
    {
        Name = serverName;
        Error = error;
        Timestamp = DateTimeOffset.Now;
    }

    /// <summary>
    /// 연결 플러그인의 이름입니다.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 연결 해제의 원인이 된 오류 (정상 해제인 경우 null)
    /// </summary>
    public Exception? Error { get; }

    /// <summary>
    /// 연결 해제 시간
    /// </summary>
    public DateTimeOffset Timestamp { get; }
}
