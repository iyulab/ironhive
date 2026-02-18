namespace IronHive.Storages.Qdrant;

/// <summary>
/// Qdrant 벡터 데이터베이스에 연결하기 위한 설정 클래스입니다.
/// 호스트, 포트, 인증 키, HTTPS 여부 및 gRPC 타임아웃을 구성할 수 있습니다.
/// </summary>
public class QdrantConfig
{
    /// <summary>
    /// Qdrant 서버의 호스트명 또는 IP 주소입니다.
    /// 기본값은 "localhost"입니다.
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Qdrant 서버의 포트 번호입니다.
    /// 기본값은 6334입니다.
    /// </summary>
    public int Port { get; set; } = 6334;

    /// <summary>
    /// Qdrant API에 접근하기 위한 인증 키입니다.
    /// 비어 있으면 인증 없이 접근합니다.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Qdrant와의 통신에 HTTPS를 사용할지 여부를 나타냅니다.
    /// 기본값은 false입니다.
    /// </summary>
    public bool Https { get; set; }

    /// <summary>
    /// gRPC 호출 시 적용할 타임아웃 시간입니다.
    /// 기본값은 100초입니다.
    /// </summary>
    public TimeSpan GrpcTimeout { get; set; } = TimeSpan.FromSeconds(100);
}