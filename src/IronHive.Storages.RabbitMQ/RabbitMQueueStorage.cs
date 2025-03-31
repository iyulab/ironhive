using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using IronHive.Abstractions.Memory;

namespace IronHive.Storages.RabbitMQ;

public class RabbitMQueueStorage : IQueueStorage
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _queueName;

    public RabbitMQueueStorage(RabbitMQConfig config)
    {
        _queueName = config.QueueName 
            ?? throw new ArgumentNullException("Queue name is required", nameof(config.QueueName));

        (_connection, _channel) = CreateAsync(config).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task EnqueueAsync<T>(T item, CancellationToken cancellationToken = default)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(item));

        await _channel.BasicPublishAsync(
            exchange: "",
            routingKey: _queueName,
            mandatory: false,
            body: body,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<T?> DequeueAsync<T>(CancellationToken cancellationToken = default)
    {
        var result = await _channel.BasicGetAsync(_queueName, autoAck: false, cancellationToken);
        if (result == null)
            return default;

        try
        {
            var body = result.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var item = JsonSerializer.Deserialize<T>(message);

            // 메시지 확인 응답
            await _channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken);

            return item;
        }
        catch
        {
            // 처리 실패 시 메시지 재입력
            await _channel.BasicNackAsync(result.DeliveryTag, multiple: false, requeue: true, cancellationToken);
            return default;
        }
    }

    /// <inheritdoc />
    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        var result = await _channel.QueueDeclarePassiveAsync(_queueName, cancellationToken);
        return (int)result.MessageCount;
    }

    /// <inheritdoc />
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await _channel.QueuePurgeAsync(_queueName, cancellationToken);
    }

    // 큐 생성
    private async Task<(IConnection, IChannel)> CreateAsync(RabbitMQConfig config)
    {
        // 설정을 이용해 ConnectionFactory 생성
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

        // 비동기로 연결 및 채널 생성
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        // 메인 큐 선언
        await channel.QueueDeclareAsync(
            queue: config.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                { "x-message-ttl", config.MessageTTLSecs * 1000 } // 밀리초 단위로 변환
            },
            noWait: false
        );

        return (connection, channel);
    }
}
