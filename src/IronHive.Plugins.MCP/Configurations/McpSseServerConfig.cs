using System.Text.Json.Serialization;

namespace IronHive.Plugins.MCP.Configurations;

/// <summary>
/// Server-Sent Events(SSE) 기반의 MCP 서버 구현입니다.
/// HTTP를 통한 서버-클라이언트 단방향 실시간 이벤트 스트리밍을 사용하여 MCP 통신을 처리합니다.
/// 웹 기반 환경에서 AI 모델과 외부 서비스 간의 통합에 적합합니다.
/// </summary>
public class McpSseServerConfig : IMcpServerConfig
{
    /// <summary>
    /// SSE 연결을 설정할 MCP 서버의 HTTP 엔드포인트 URL입니다.
    /// 예: "http://localhost:8000/sse"
    /// </summary>
    [JsonPropertyName("url")]
    public required Uri Endpoint { get; set; }

    /// <summary>
    /// HTTP 요청 시 포함할 추가 헤더 컬렉션입니다.
    /// 인증, 콘텐츠 타입 지정 등에 사용됩니다.
    /// </summary>
    [JsonPropertyName("headers")]
    public Dictionary<string, string>? AdditionalHeaders { get; set; }

    /// <summary>
    /// SSE 서버 연결 시도 시 최대 대기 시간입니다.
    /// 지정된 시간 내에 연결이 설정되지 않으면 타임아웃 예외가 발생합니다.
    /// 기본값은 30초입니다.
    /// </summary>
    [JsonPropertyName("timeout")]
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// HTTP 통신에서 스트리밍 방식 사용 여부를 지정합니다.
    /// true로 설정 시 청크 단위의 스트리밍 응답을 처리하며, 대용량 데이터 전송에 효율적입니다.
    /// 기본값은 false입니다.
    /// </summary>
    [JsonPropertyName("stream")]
    public bool UseStreamableHttp { get; set; } = false;

    /// <summary>
    /// MCP 클라이언트가 생성될 때 자동으로 서버에 연결할지 여부를 나타냅니다.
    /// 기본 값은 true입니다.
    /// </summary>
    public bool AutoConnectOnCreated { get; set; } = true;

    public override bool Equals(object? obj)
    {
        return obj is McpSseServerConfig server &&
               EqualityComparer<Uri>.Default.Equals(Endpoint, server.Endpoint) &&
               EqualityComparer<Dictionary<string, string>?>.Default.Equals(AdditionalHeaders, server.AdditionalHeaders) &&
               ConnectionTimeout.Equals(server.ConnectionTimeout) &&
               UseStreamableHttp == server.UseStreamableHttp;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Endpoint, AdditionalHeaders, ConnectionTimeout, UseStreamableHttp);
    }
}
