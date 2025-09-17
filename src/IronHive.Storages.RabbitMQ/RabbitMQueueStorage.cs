using IronHive.Abstractions.Queue;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace IronHive.Storages.RabbitMQ;

/// <summary>
/// RabbitMQ를 사용하여 큐 스토리지를 구현한 클래스입니다.
/// </summary>
public class RabbitMQueueStorage : IQueueStorage
{
    private readonly RabbitMQConfig _config;

    private IConnection? _conn;

    // 채널을 역할별로 분리
    private IChannel? _pubChannel;   // 발행 전용
    private IChannel? _mgmChannel;   // 관리 전용 (BasicGet/Count/Purge)

    // 발행 직렬화 (채널은 스레드-세이프 아님)
    private readonly SemaphoreSlim _pubLock = new(1, 1);

    public RabbitMQueueStorage(RabbitMQConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// 현재 설정된 큐 이름을 나타냅니다.
    /// </summary>
    public string QueueName => _config.QueueName;

    /// <inheritdoc />
    public void Dispose()
    {
        _pubChannel?.Dispose();
        _mgmChannel?.Dispose();

        _conn?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IQueueConsumer> CreateConsumerAsync<T>(
        Func<IQueueMessage<T>, Task> onReceived, 
        CancellationToken cancellationToken = default)
    {
        // 소비 전용 채널 생성
        var conn = await GetOrCreateConnectionAsync(cancellationToken);
        var channel = await conn.CreateChannelAsync(new CreateChannelOptions(false, false, null, 1), cancellationToken);
        await DeclareQueueAsync(channel, cancellationToken);
        await channel.BasicQosAsync(0, 1, false, cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (ch, ea) =>
        {
            try
            {
                var body = JsonSerializer.Deserialize<T>(ea.Body.ToArray(), _config.JsonOptions)
                    ?? throw new JsonException("Failed to deserialize message.");
                var msg = new RabbitMQMessage<T>(channel)
                {
                    Body = body,
                    DeliveryTag = ea.DeliveryTag,
                };
                await onReceived(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Consumer error: {ex.Message}");
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };
        return new RabbitMQConsumer(consumer)
        {
            QueueName = QueueName,
        };
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        var channel = await GetOrCreateMgmChannelAsync(cancellationToken);
        var count = await channel.MessageCountAsync(QueueName, cancellationToken);
        return (int)count;
    }

    /// <inheritdoc />
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        var channel = await GetOrCreateMgmChannelAsync(cancellationToken);
        await channel.QueuePurgeAsync(QueueName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task EnqueueAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        var channel = await GetOrCreatePubChannelAsync(cancellationToken);

        var body = JsonSerializer.SerializeToUtf8Bytes(message, _config.JsonOptions);
        var props = new BasicProperties
        {
            MessageId = Guid.NewGuid().ToString(),
            Persistent = true, // delivery mode 2
            ContentType = "application/json",
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
        };

        // 발행 타임아웃, 무한 대기 방지
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        
        await _pubLock.WaitAsync(cancellationToken);
        try
        {
            await channel.BasicPublishAsync(
                exchange: string.Empty,     // 기본 Direct Exchange를 사용하여 큐에 메시지 게시
                routingKey: QueueName,
                mandatory: false,           // 메시지가 라우팅되지 않아도 오류를 발생시키지 않음(true일시 Basic.Return 이벤트 필요)
                body: body,
                basicProperties: props,
                cancellationToken: cancellationToken);
        }
        finally
        {
            _pubLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<IQueueMessage<T>?> DequeueAsync<T>(CancellationToken cancellationToken = default)
    {
        var channel = await GetOrCreateMgmChannelAsync(cancellationToken);

        var result = await channel.BasicGetAsync(QueueName, autoAck: false, cancellationToken);
        if (result == null)
            return null;

        var body = result.Body.ToArray();
        var message = JsonSerializer.Deserialize<T>(body, _config.JsonOptions)
            ?? throw new JsonException("Failed to deserialize message.");

        return new RabbitMQMessage<T>(channel)
        {
            Body = message,
            DeliveryTag = result.DeliveryTag,
        };
    }

    /// <summary> 발행용 채널 </summary>
    private async Task<IChannel> GetOrCreatePubChannelAsync(CancellationToken ct)
    {
        var conn = await GetOrCreateConnectionAsync(ct);
        if (_pubChannel is { IsOpen: true }) return _pubChannel;

        _pubChannel?.Dispose();
        _pubChannel = await conn.CreateChannelAsync(new CreateChannelOptions(true, true), ct);

        await DeclareQueueAsync(_pubChannel, ct);
        return _pubChannel;
    }

    /// <summary> 관리용 채널 (BasicGet, Count, Purge 등) </summary>
    private async Task<IChannel> GetOrCreateMgmChannelAsync(CancellationToken ct)
    {
        var conn = await GetOrCreateConnectionAsync(ct);
        if (_mgmChannel is { IsOpen: true }) return _mgmChannel;

        _mgmChannel?.Dispose();
        _mgmChannel = await conn.CreateChannelAsync(null, ct);

        await DeclareQueueAsync(_mgmChannel, ct);
        return _mgmChannel;
    }

    /// <summary> RabbitMQ 커넥션 생성/재생성 </summary>
    private async Task<IConnection> GetOrCreateConnectionAsync(CancellationToken ct)
    {
        if (_conn is { IsOpen: true })
            return _conn;

        // 기존 채널 정리 (연결 재생성 시)
        _pubChannel?.Dispose(); _pubChannel = null;
        _mgmChannel?.Dispose(); _mgmChannel = null;

        _conn?.Dispose();
        var factory = new ConnectionFactory
        {
            HostName = _config.Host,
            Port = _config.Port,
            UserName = _config.UserName,
            Password = _config.Password,
            VirtualHost = _config.VirtualHost,
            Ssl = { Enabled = _config.SslEnabled },
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
        };

        _conn = await factory.CreateConnectionAsync(ct);
        return _conn;
    }

    /// <summary> RabbitMQ 큐를 선언합니다. </summary>
    private async Task DeclareQueueAsync(IChannel channel, CancellationToken ct)
    {
        var args = new Dictionary<string, object?>
        {
            { "x-queue-type", "quorum" }, // 고가용성/내구성 제공 최신 큐 타입
        };
        if (_config.MessageTTL.HasValue && _config.MessageTTL > 0)
        {
            args["x-message-ttl"] = _config.MessageTTL.Value;
        }

        await channel.QueueDeclareAsync(
            queue: _config.QueueName,
            durable: true,      // 서버 재시작 후에도 큐가 유지되도록 설정
            exclusive: false,   // 여러 커넥션에서 접근 가능
            autoDelete: false,  // 마지막 소비자가 연결을 끊어도 큐가 삭제되지 않도록 설정
            arguments: args,
            cancellationToken: ct);
    }
}
