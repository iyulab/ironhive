using System.Text.Json;
using RabbitMQ.Client;
using IronHive.Abstractions.Queue;

namespace IronHive.Storages.RabbitMQ;

/// <summary>
/// RabbitMQ를 사용하여 큐 스토리지를 구현한 클래스입니다.
/// </summary>
public class RabbitMQueueStorage : IQueueStorage
{
    private readonly RabbitMQConfig _config;
    private readonly string _queueName;
    private readonly JsonSerializerOptions _jsonOptions;

    private IConnection? _conn;
    private IChannel? _channel;

    public RabbitMQueueStorage(RabbitMQConfig config)
    {
        _config = config;
        _queueName = config.QueueName;
        _jsonOptions = config.JsonOptions;
    }

    /// <inheritdoc />
    public required string StorageName { get; init; }

    /// <inheritdoc />
    public void Dispose()
    {
        _conn?.Dispose();
        _channel?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        var channel = await GetOrCreateChannelAsync();
        var count = await channel.MessageCountAsync(_queueName, cancellationToken);
        return (int)count;
    }

    /// <inheritdoc />
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        var channel = await GetOrCreateChannelAsync();
        await channel.QueuePurgeAsync(_queueName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task EnqueueAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        var channel = await GetOrCreateChannelAsync();

        var body = JsonSerializer.SerializeToUtf8Bytes(message, _jsonOptions);
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: _queueName,
            mandatory: false,
            body: body,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<QueueMessage<T>?> DequeueAsync<T>(CancellationToken cancellationToken = default)
    {
        var channel = await GetOrCreateChannelAsync();

        var result = await channel.BasicGetAsync(_queueName, autoAck: false, cancellationToken);
        if (result == null)
            return null;

        var body = result.Body.ToArray();
        var message = JsonSerializer.Deserialize<T>(body, _jsonOptions)
            ?? throw new JsonException("Failed to deserialize message.");

        return new QueueMessage<T>
        {
            Payload = message,
            Tag = result.DeliveryTag
        };
    }

    /// <inheritdoc />
    public async Task AckAsync(object tag, CancellationToken cancellationToken = default)
    {
        var channel = await GetOrCreateChannelAsync();

        if (tag is not ulong rabbitTag)
            throw new InvalidOperationException("Invalid rabbitMq tag.");

        // 동일 채널의 Tag에 대해서만 Ack 처리 가능
        await channel.BasicAckAsync(rabbitTag, false, cancellationToken);
    }

    /// <inheritdoc />
    public async Task NackAsync(object tag, bool requeue = false, CancellationToken cancellationToken = default)
    {
        var channel = await GetOrCreateChannelAsync();

        if (tag is not ulong rabbitTag)
            throw new InvalidOperationException("Invalid rabbitMq tag.");

        // 동일 채널의 Tag에 대해서만 Ack 처리 가능
        await channel.BasicNackAsync(rabbitTag, false, requeue, cancellationToken);
    }

    /// <summary>
    /// RabbitMQ 커넥션과 채널이 열려 있는지 확인하고, 필요시 재연결합니다.
    /// </summary>
    private async Task<IChannel> GetOrCreateChannelAsync()
    {
        if (_conn == null || !_conn.IsOpen)
        {
            _channel?.Dispose();
            _conn?.Dispose();
            _conn = await CreateConnectionAsync(_config);
        }

        if (_channel == null || !_channel.IsOpen)
        {
            _channel?.Dispose();
            _channel = await CreateChannelAsync(_conn, _config);
        }

        return _channel;
    }

    /// <summary>
    /// RabbitMQ 채널 생성
    /// </summary>
    private static async Task<IChannel> CreateChannelAsync(IConnection conn, RabbitMQConfig config)
    {
        var channel = await conn.CreateChannelAsync();
        var args = new Dictionary<string, object?>();
        
        if (config.MessageTTLSecs.HasValue && config.MessageTTLSecs > 0)
        {
            // 메시지 TTL 설정, ms 단위로 변환
            args.Add("x-message-ttl", config.MessageTTLSecs.Value * 1_000);
        }

        channel.QueueDeclareAsync(
            queue: config.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: args
        ).Wait();
        return channel;
    }

    /// <summary>
    /// RabbitMQ 커넥션 생성
    /// </summary>
    private static async Task<IConnection> CreateConnectionAsync(RabbitMQConfig config)
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
}
