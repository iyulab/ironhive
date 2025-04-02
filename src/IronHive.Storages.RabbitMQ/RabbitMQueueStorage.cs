using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using IronHive.Abstractions.Memory;
using System.Net.Http.Headers;
using System.Threading;
using System.Xml.Linq;

namespace IronHive.Storages.RabbitMQ;

public class RabbitMQueueStorage : IQueueStorage
{
    private readonly IConnection _conn;
    private readonly RabbitMQConfig _config;
    private readonly string _queueName;

    public RabbitMQueueStorage(RabbitMQConfig config)
    {
        _config = config;
        _queueName = config.QueueName;
        _conn = CreateConnection(config);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _conn?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task EnqueueAsync(string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentNullException(nameof(message));

        var body = Encoding.UTF8.GetBytes(message);
        using var channel = await _conn.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: _queueName,
            mandatory: false,
            body: body,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<string?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        using var channel = await _conn.CreateChannelAsync(cancellationToken: cancellationToken);
        var result = await channel.BasicGetAsync(_queueName, autoAck: false, cancellationToken);
        if (result == null)
            return default;

        try
        {
            var body = result.Body.ToArray();
            var item = Encoding.UTF8.GetString(body);

            // 메시지 확인 응답
            await channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken);

            return item;
        }
        catch
        {
            // 처리 실패 시 메시지 재입력
            await channel.BasicNackAsync(result.DeliveryTag, multiple: false, requeue: true, cancellationToken);
            return default;
        }
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        using var channel = await _conn.CreateChannelAsync(cancellationToken: cancellationToken);
        var count = await channel.MessageCountAsync(_queueName, cancellationToken);
        return (int)count;
    }

    /// <inheritdoc />
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        using var channel = await _conn.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.QueuePurgeAsync(_queueName, cancellationToken);
    }

    // RabbitMQ 연결 및 채널 생성
    private IConnection CreateConnection(RabbitMQConfig config)
    {
        var factory = new ConnectionFactory
        {
            HostName = config.Host,
            Port = config.Port,
            UserName = config.Username,
            Password = config.Password,
            VirtualHost = config.VirtualHost,
            Ssl = {
                Enabled = config.SslEnabled,
            }
        };

        // 동기로 연결 및 큐 생성
        var conn = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        var channel = conn.CreateChannelAsync().GetAwaiter().GetResult();
        channel.QueueDeclareAsync(
            queue: config.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                { "x-message-ttl", _config.MessageTTLSecs * 1000 } // 밀리초 단위로 변환
            }
        ).Wait();

        return conn;
    }
}
