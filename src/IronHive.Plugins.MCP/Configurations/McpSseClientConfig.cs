using System.Text.Json.Serialization;

namespace IronHive.Plugins.MCP.Configurations;

/// <summary>
/// Server-Sent Events(SSE) 기반의 MCP 서버 구현입니다.
/// HTTP를 통한 서버-클라이언트 단방향 실시간 이벤트 스트리밍을 사용하여 MCP 통신을 처리합니다.
/// 웹 기반 환경에서 AI 모델과 외부 서비스 간의 통합에 적합합니다.
/// </summary>
public class McpSseClientConfig : IMcpClientConfig
{
    /// <inheritdoc />
    public required string ServerName { get; set; }

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

    public override bool Equals(object? obj)
    {
        return obj is McpSseClientConfig server &&
               ServerName == server.ServerName &&
               EqualityComparer<Uri>.Default.Equals(Endpoint, server.Endpoint) &&
               EqualityComparer<Dictionary<string, string>?>.Default.Equals(AdditionalHeaders, server.AdditionalHeaders) &&
               ConnectionTimeout.Equals(server.ConnectionTimeout);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ServerName, Endpoint, AdditionalHeaders, ConnectionTimeout);
    }
}
