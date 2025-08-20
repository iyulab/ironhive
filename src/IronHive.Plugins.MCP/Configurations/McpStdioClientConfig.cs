using System.Text.Json.Serialization;

namespace IronHive.Plugins.MCP.Configurations;

/// <summary>
/// 표준 입출력(STDIO) 기반의 MCP 서버 구현입니다.
/// 명령줄 프로세스를 통해 표준 입력(stdin)과 출력(stdout)을 사용하여 MCP 통신을 처리합니다.
/// 로컬 환경에서 AI 모델과 외부 도구 간의 통합에 적합합니다.
/// </summary>
public class McpStdioClientConfig : IMcpClientConfig
{
    /// <summary>
    /// MCP 서버로 실행할 외부 프로그램 또는 명령을 지정합니다.
    /// 이 명령은 MCP 프로토콜을 통한 통신을 지원해야 합니다.
    /// </summary>
    [JsonPropertyName("command")]
    public required string Command { get; set; }

    /// <summary>
    /// 명령 실행 시 전달할 명령줄 인수 목록입니다.
    /// 서버 구성과 동작을 제어하는 데 사용됩니다.
    /// </summary>
    [JsonPropertyName("args")]
    public IEnumerable<string>? Arguments { get; set; }

    /// <summary>
    /// 서버 프로세스 실행 시 설정할 환경 변수 컬렉션입니다.
    /// 서버 동작을 구성하거나 필요한 정보를 프로세스에 전달하는 데 사용됩니다.
    /// </summary>
    [JsonPropertyName("env")]
    public Dictionary<string, string?>? EnvironmentVariables { get; set; }

    /// <summary>
    /// 서버 종료 요청 후 강제 종료까지 대기하는 최대 시간입니다.
    /// 기본값은 5초입니다.
    /// </summary>
    [JsonPropertyName("timeout")]
    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 서버 프로세스가 실행될 작업 디렉터리 경로입니다.
    /// 상대 경로 참조 및 리소스 액세스에 영향을 줍니다.
    /// </summary>
    [JsonPropertyName("directory")]
    public string? WorkingDirectory { get; set; }

    /// <summary>
    /// MCP 클라이언트가 생성될 때 자동으로 서버에 연결할지 여부를 나타냅니다.
    /// 기본값은 false입니다.
    /// </summary>
    public bool AutoConnectOnCreated { get; set; } = false;

    public override bool Equals(object? obj)
    {
        return obj is McpStdioClientConfig server &&
               Command == server.Command &&
               EqualityComparer<IEnumerable<string>?>.Default.Equals(Arguments, server.Arguments) &&
               EqualityComparer<Dictionary<string, string?>?>.Default.Equals(EnvironmentVariables, server.EnvironmentVariables) &&
               ShutdownTimeout.Equals(server.ShutdownTimeout) &&
               WorkingDirectory == server.WorkingDirectory;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Command, Arguments, EnvironmentVariables, ShutdownTimeout, WorkingDirectory);
    }
}
