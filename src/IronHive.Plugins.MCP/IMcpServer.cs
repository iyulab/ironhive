using System.Text.Json.Serialization;

namespace IronHive.Plugins.MCP;

/// <summary>
/// Model Context Protocol(MCP) 서버를 위한 공통 인터페이스입니다.
/// MCP는 AI 모델과 외부 도구, 시스템, 데이터 소스 간의 통신을 표준화하는 프로토콜입니다.
/// 여러 유형의 MCP 서버 구현을 위한 다형성을 지원합니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(McpStdioServer), "stdio")]
[JsonDerivedType(typeof(McpSseServer), "sse")]
public interface IMcpServer
{ }

/// <summary>
/// 표준 입출력(STDIO) 기반의 MCP 서버 구현입니다.
/// 명령줄 프로세스를 통해 표준 입력(stdin)과 출력(stdout)을 사용하여 MCP 통신을 처리합니다.
/// 로컬 환경에서 AI 모델과 외부 도구 간의 통합에 적합합니다.
/// </summary>
public class McpStdioServer : IMcpServer
{
    /// <summary>
    /// MCP 서버로 실행할 외부 프로그램 또는 명령을 지정합니다.
    /// 이 명령은 MCP 프로토콜을 통한 통신을 지원해야 합니다.
    /// </summary>
    public required string Command { get; set; }

    /// <summary>
    /// 명령 실행 시 전달할 명령줄 인수 목록입니다.
    /// 서버 구성과 동작을 제어하는 데 사용됩니다.
    /// </summary>
    public IEnumerable<string>? Arguments { get; set; }

    /// <summary>
    /// 서버 프로세스 실행 시 설정할 환경 변수 컬렉션입니다.
    /// 서버 동작을 구성하거나 필요한 정보를 프로세스에 전달하는 데 사용됩니다.
    /// </summary>
    public Dictionary<string, string?>? EnvironmentVariables { get; set; }

    /// <summary>
    /// 서버 종료 요청 후 강제 종료까지 대기하는 최대 시간입니다.
    /// 기본값은 5초입니다.
    /// </summary>
    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 서버 프로세스가 실행될 작업 디렉터리 경로입니다.
    /// 상대 경로 참조 및 리소스 액세스에 영향을 줍니다.
    /// </summary>
    public string? WorkingDirectory { get; set; }
}

/// <summary>
/// Server-Sent Events(SSE) 기반의 MCP 서버 구현입니다.
/// HTTP를 통한 서버-클라이언트 단방향 실시간 이벤트 스트리밍을 사용하여 MCP 통신을 처리합니다.
/// 웹 기반 환경에서 AI 모델과 외부 서비스 간의 통합에 적합합니다.
/// </summary>
public class McpSseServer : IMcpServer
{
    /// <summary>
    /// SSE 연결을 설정할 MCP 서버의 HTTP 엔드포인트 URL입니다.
    /// 예: "http://localhost:8000/sse"
    /// </summary>
    public required Uri Endpoint { get; set; }

    /// <summary>
    /// HTTP 요청 시 포함할 추가 헤더 컬렉션입니다.
    /// 인증, 콘텐츠 타입 지정 등에 사용됩니다.
    /// </summary>
    public Dictionary<string, string>? AdditionalHeaders { get; set; }

    /// <summary>
    /// SSE 서버 연결 시도 시 최대 대기 시간입니다.
    /// 지정된 시간 내에 연결이 설정되지 않으면 타임아웃 예외가 발생합니다.
    /// 기본값은 30초입니다.
    /// </summary>
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// HTTP 통신에서 스트리밍 방식 사용 여부를 지정합니다.
    /// true로 설정 시 청크 단위의 스트리밍 응답을 처리하며, 대용량 데이터 전송에 효율적입니다.
    /// 기본값은 false입니다.
    /// </summary>
    public bool UseStreamableHttp { get; set; } = false;
}
