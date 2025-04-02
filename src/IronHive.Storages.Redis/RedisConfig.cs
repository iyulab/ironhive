using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronHive.Storages.Redis;

/// <summary>
/// Redis 연결을 위한 설정 클래스입니다.
/// </summary>
public class RedisConfig
{
    /// <summary>
    /// Redis 연결 문자열입니다. 설정 시 개별 속성보다 우선 적용됩니다.
    /// 예: "localhost:6379,password=yourpassword,ssl=true"
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Redis 서버의 호스트 주소입니다. 기본값은 "localhost".
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Redis 서버의 포트 번호입니다. 기본값은 6379입니다.
    /// </summary>
    public int Port { get; set; } = 6379;

    /// <summary>
    /// Redis 서버에 접근하기 위한 비밀번호입니다.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 연결 실패 시 연결을 중단할지 여부입니다. 기본값은 true입니다.
    /// </summary>
    public bool AbortOnConnectFail { get; set; } = true;

    /// <summary>
    /// 연결 타임아웃(밀리초)입니다. 기본값은 5000ms입니다.
    /// </summary>
    public int ConnectTimeout { get; set; } = 5000;

    /// <summary>
    /// 연결 재시도 횟수입니다. 기본값은 3입니다.
    /// </summary>
    public int ConnectRetry { get; set; } = 3;

    /// <summary>
    /// 기본 데이터베이스 번호입니다. 기본값은 null(0번 DB)입니다.
    /// </summary>
    public int? DefaultDatabase { get; set; }

    /// <summary>
    /// SSL/TLS 암호화 사용 여부입니다. 기본값은 false입니다.
    /// </summary>
    public bool Ssl { get; set; } = false;

    /// <summary>
    /// 동기 작업 타임아웃(밀리초)입니다. 기본값은 5000ms입니다.
    /// </summary>
    public int SyncTimeout { get; set; } = 5000;

    /// <summary>
    /// 비동기 작업 타임아웃(밀리초)입니다. 기본값은 SyncTimeout 값입니다.
    /// </summary>
    public int AsyncTimeout { get; set; } = 5000;

    /// <summary>
    /// 소켓 유지 메시지 전송 주기(초)입니다. 기본값은 60초입니다.
    /// </summary>
    public int KeepAlive { get; set; } = 60;

    /// <summary>
    /// 연결에 사용할 클라이언트 이름입니다.
    /// </summary>
    public string? ClientName { get; set; }

    /// <summary>
    /// 값 시리얼라이즈시 사용하는 JsonSerializerOptions입니다.
    /// <summary>
    public JsonSerializerOptions JsonOptions { get; set; } = new JsonSerializerOptions
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
