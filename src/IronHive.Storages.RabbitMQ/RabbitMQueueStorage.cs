using System.Text.Json;
using RabbitMQ.Client;
using IronHive.Abstractions.Memory;

namespace IronHive.Storages.RabbitMQ;

/// <summary>
/// RabbitMQ를 사용하여 큐 스토리지를 구현한 클래스입니다.
/// </summary>
public class RabbitMQueueStorage<T> : IQueueStorage<T>
{
    private IConnection _conn;
    private IChannel _channel;
    private bool _isOpen => (_conn.IsOpen && _channel.IsOpen);

    private readonly RabbitMQConfig _config;
    private readonly string _queueName;
    private readonly JsonSerializerOptions _jsonOptions;

    public RabbitMQueueStorage(RabbitMQConfig config)
    {
        _config = config;
        _queueName = config.QueueName;
        _jsonOptions = config.JsonOptions;
        _conn = CreateConnectionAsync(config).Result;
        _channel = CreateChannelAsync(_conn, _config).Result;
    }

    public void Dispose()
    {
        _conn?.Dispose();
        _channel?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task EnqueueAsync(T message, CancellationToken cancellationToken = default)
    {
        if (!_isOpen)
            await ReConnectAsync();

        var body = JsonSerializer.SerializeToUtf8Bytes(message, _jsonOptions);
        await _channel.BasicPublishAsync(
            exchange: "",
            routingKey: _queueName,
            mandatory: false,
            body: body,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<TaggedMessage<T>?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        if (!_isOpen)
            await ReConnectAsync();

        var result = await _channel.BasicGetAsync(_queueName, autoAck: false, cancellationToken);
        if (result == null)
        {
            return null;
        }

        // 큐 안에 다른 타입의 메시지가 있을 경우 일단 Throw
        // IQueueStorage<T>로 구현 변경, 생각 해보기
        var body = result.Body.ToArray();
        var message = JsonSerializer.Deserialize<T>(body, _jsonOptions)
            ?? throw new JsonException("Failed to deserialize message.");

        return new TaggedMessage<T>
        {
            Message = message,
            AckTag = result.DeliveryTag
        };
    }

    /// <inheritdoc />
    public async Task AckAsync(object ackTag, CancellationToken cancellationToken = default)
    {
        if (!_isOpen)
            await ReConnectAsync();

        if (ackTag is not ulong rabbitTag)
            throw new InvalidOperationException("Invalid ack tag.");

        // 동일 채널의 Tag에 대해서만 Ack 처리 가능
        await _channel.BasicAckAsync(rabbitTag, false, cancellationToken);
    }

    /// <inheritdoc />
    public async Task NackAsync(object ackTag, bool requeue = false, CancellationToken cancellationToken = default)
    {
        if (!_isOpen)
            await ReConnectAsync();

        if (ackTag is not ulong rabbitTag)
            throw new InvalidOperationException("Invalid ack tag.");

        // 동일 채널의 Tag에 대해서만 Ack 처리 가능
        await _channel.BasicNackAsync(rabbitTag, false, requeue, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        if (!_isOpen)
            await ReConnectAsync();

        var count = await _channel.MessageCountAsync(_queueName, cancellationToken);
        return (int)count;
    }

    /// <inheritdoc />
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        if (!_isOpen)
            await ReConnectAsync();

        await _channel.QueuePurgeAsync(_queueName, cancellationToken);
    }

    // RabbitMQ 커넥션 생성
    private async Task<IConnection> CreateConnectionAsync(RabbitMQConfig config)
    {
        var factory = new ConnectionFactory
        {
            HostName = config.Host,
            Port = config.Port,
            UserName = config.Username,
            Password = config.Password,
            VirtualHost = config.VirtualHost,
            Ssl = { Enabled = config.SslEnabled }
        };

        var conn = await factory.CreateConnectionAsync();
        return conn;
    }

    // RabbitMQ 채널 생성
    private async Task<IChannel> CreateChannelAsync(IConnection conn, RabbitMQConfig config)
    {
        var channel = await conn.CreateChannelAsync();
        var args = new Dictionary<string, object?>();
        
        if (_config.MessageTTLSecs.HasValue && _config.MessageTTLSecs > 0)
        {
            // 메시지 TTL 설정, ms 단위로 변환
            args.Add("x-message-ttl", _config.MessageTTLSecs.Value * 1_000);
        }

        channel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: args
        ).Wait();
        return channel;
    }

    // RabbitMQ 커넥션 및 채널 재연결
    private async Task ReConnectAsync()
    {
        if (!_conn.IsOpen)
        {
            _channel.Dispose();
            _conn.Dispose();
            _conn = await CreateConnectionAsync(_config);
        }

        if (!_channel.IsOpen)
        {
            _channel.Dispose();
            _channel = await CreateChannelAsync(_conn, _config);
        }
    }
}
