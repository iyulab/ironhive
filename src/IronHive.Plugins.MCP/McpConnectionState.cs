namespace IronHive.Plugins.MCP;

/// <summary>
/// MCP (ModelContext Protocol) 클라이언트의 연결 상태를 나타내는 열거형입니다.
/// </summary>
public enum McpConnectionState
{
    /// <summary>
    /// 클라이언트가 MCP 서버에 연결을 시도 중인 상태입니다.
    /// </summary>
    Connecting,

    /// <summary>
    /// 클라이언트가 MCP 서버에 성공적으로 연결된 상태입니다.
    /// </summary>
    Connected,

    /// <summary>
    /// 클라이언트가 MCP 서버와의 연결을 해제하는 중인 상태입니다.
    /// </summary>
    Disconnecting,

    /// <summary>
    /// 연결이 끊어진 상태입니다. 클라이언트가 MCP 서버와 연결되어 있지 않습니다.
    /// </summary>
    Disconnected,
}
