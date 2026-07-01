namespace IronHive.Plugins.MCP.Configurations;

/// <summary>
/// HTTP 기반의 MCP 서버 연결 설정입니다.
/// SSE(Server-Sent Events) 및 Streamable HTTP 전송 방식을 자동으로 감지하여 연결합니다.
/// </summary>
public class McpHttpClientConfig : IMcpClientConfig
{
    /// <inheritdoc />
    public required string ServerName { get; set; }

    /// <summary>
    /// MCP 서버의 HTTP 엔드포인트 URL입니다.
    /// </summary>
    public required Uri Endpoint { get; set; }

    /// <summary>
    /// HTTP 요청 시 포함할 추가 헤더 컬렉션입니다.
    /// </summary>
    public Dictionary<string, string>? AdditionalHeaders { get; set; }

    /// <summary>
    /// 서버 연결 시도 시 최대 대기 시간입니다.
    /// 기본값은 30초입니다.
    /// </summary>
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// OAuth 2.0 인증 설정입니다.
    /// null이면 인증 없이 연결합니다.
    /// </summary>
    public McpHttpOAuthConfig? OAuth { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is McpHttpClientConfig other &&
               ServerName == other.ServerName &&
               EqualityComparer<Uri>.Default.Equals(Endpoint, other.Endpoint) &&
               EqualityComparer<Dictionary<string, string>?>.Default.Equals(AdditionalHeaders, other.AdditionalHeaders) &&
               ConnectionTimeout.Equals(other.ConnectionTimeout);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ServerName, Endpoint, AdditionalHeaders, ConnectionTimeout);
    }
}

/// <summary>
/// HTTP 기반 MCP 서버 연결에 사용되는 OAuth 2.0 인증 설정입니다.
/// </summary>
public class McpHttpOAuthConfig
{
    /// <summary>
    /// OAuth 인증 후 리디렉션될 URI입니다.
    /// </summary>
    public required Uri RedirectUri { get; set; }

    /// <summary>
    /// OAuth 클라이언트 ID입니다.
    /// 지정하지 않으면 Dynamic Client Registration을 시도합니다.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// OAuth 클라이언트 시크릿입니다.
    /// Public client 또는 PKCE 사용 시 생략할 수 있습니다.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// 요청할 OAuth 스코프 목록입니다.
    /// 지정하지 않으면 서버의 Protected Resource Metadata에 정의된 스코프를 사용합니다.
    /// </summary>
    public IList<string>? Scopes { get; set; }

    /// <summary>
    /// OAuth 인증 요청의 쿼리 스트링에 포함할 추가 파라미터입니다.
    /// redirect_uri 등 자동 설정 파라미터는 덮어쓸 수 없습니다.
    /// </summary>
    public Dictionary<string, string>? AdditionalParameters { get; set; }
}
