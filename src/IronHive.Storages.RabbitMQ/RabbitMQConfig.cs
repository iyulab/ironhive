using System.Text.Json;

namespace IronHive.Storages.RabbitMQ;

/// <summary>
/// RabbitMQ 연결 및 메시지 큐 설정을 위한 구성 클래스
/// </summary>
public class RabbitMQConfig
{
    /// <summary>
    /// RabbitMQ 호스트 이름, Default: "127.0.0.1"
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// 연결을 위한 TCP 포트, Default: 5672
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// 인증에 사용할 사용자 이름, Default: "guest"
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// 인증에 사용할 비밀번호, Default: "guest"
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// 사용할 Queue 이름
    /// </summary>
    public required string QueueName { get; set; }

    /// <summary>
    /// RabbitMQ 가상 호스트 이름, 예: "/"
    /// 자세한 내용은 https://www.rabbitmq.com/docs/vhosts 참고
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// 메시지의 TTL(Time-To-Live) (ms 단위 설정)    
    /// </summary>
    public int? MessageTTL { get; set; }

    /// <summary>
    /// SSL 사용 여부
    /// </summary>
    public bool SslEnabled { get; set; }

    /// <summary>
    /// Queue 메시지를 JSON으로 직렬화할 때 사용할 옵션
    /// </summary>
    public JsonSerializerOptions JsonOptions { get; set; } = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };
}
