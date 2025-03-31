using System.Text;

namespace IronHive.Storages.RabbitMQ;

public class RabbitMQConfig
{
    /// <summary>
    /// RabbitMQ 호스트 이름, 예: "127.0.0.1"
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// 연결을 위한 TCP 포트, 예: 5672
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// 인증에 사용할 사용자 이름
    /// </summary>
    public string Username { get; set; } = "guest";

    /// <summary>
    /// 인증에 사용할 비밀번호
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// 큐 이름, default: "ironhive"
    /// </summary>
    public string QueueName { get; set; } = "memory";

    /// <summary>
    /// RabbitMQ 가상 호스트 이름, 예: "/"
    /// 자세한 내용은 https://www.rabbitmq.com/docs/vhosts 참고
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// 메시지의 TTL(Time-To-Live) (초 단위)
    /// 기본값: 3600초 (1시간)
    /// </summary>
    public int MessageTTLSecs { get; set; } = 3_600;

    /// <summary>
    /// SSL 사용 여부
    /// </summary>
    public bool SslEnabled { get; set; }
}
