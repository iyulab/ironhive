namespace ConsoleTest;

/// <summary>
/// OpenAPI 클라이언트의 설정 정보를 정의하는 구성 클래스입니다.
/// 클라이언트 이름, 문서 경로, 인증 정보, 기본 헤더 및 타임아웃 설정을 포함합니다.
/// </summary>
public sealed class OpenApiClientConfig
{
    /// <summary>
    /// 클라이언트의 고유 이름입니다.
    /// 클라이언트 식별 및 관리에 사용됩니다.
    /// </summary>
    public required string ClientName { get; init; }

    /// <summary>
    /// OpenAPI 문서의 URL 또는 JSON/YAML 형식의 문자열입니다.
    /// 클라이언트가 참조할 API 명세를 지정합니다.
    /// </summary>
    public required string Document { get; init; }

    /// <summary>
    /// 인증에 필요한 비밀 정보들을 담고 있는 딕셔너리입니다.
    /// 키는 인증 스키마 이름이며, 값은 해당 인증 정보를 나타냅니다.
    /// </summary>
    public IDictionary<string, IOpenApiAuthSecret>? Secrets { get; init; }

    /// <summary>
    /// 클라이언트 요청에 기본적으로 포함될 HTTP 헤더 목록입니다.
    /// 대소문자를 구분하지 않는 키 비교를 사용하며, 기본적으로 User-Agent 헤더가 설정되어 있습니다.
    /// </summary>
    public Dictionary<string, string> DefaultHeaders { get; init; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["User-Agent"] = "IronHive Agent"
    };

    /// <summary>
    /// 클라이언트 요청의 타임아웃 시간(초)입니다.
    /// 기본값은 60초이며, 요청이 이 시간을 초과하면 취소됩니다.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 60;
}
