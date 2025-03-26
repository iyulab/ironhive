using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using IronHive.Abstractions.Memory;

namespace IronHive.Storages.RabbitMQ;

public class RabbitQueueStorage : IQueueStorage
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly RabbitConfig _config;
    private readonly string _queueName;

    public RabbitQueueStorage(RabbitConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _queueName = config.QueueName ?? throw new ArgumentNullException(nameof(config.QueueName));

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

        //await _channel.BasicPublishAsync<T>(
        //    exchange: "",
        //    routingKey: _queueName,
        //    mandatory: false,
        //    basicProperties: item,
        //    body: body,
        //    cancellationToken: cancellationToken);
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

            // Acknowledge the message
            await _channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken);

            return item;
        }
        catch
        {
            // Negative acknowledge with requeue
            await _channel.BasicNackAsync(result.DeliveryTag, multiple: false, requeue: true, cancellationToken);
            return default;
        }
    }

    /// <inheritdoc />
    public async Task<T?> PeekAsync<T>(CancellationToken cancellationToken = default)
    {
        var result = await _channel.BasicGetAsync(_queueName, autoAck: true, cancellationToken);
        if (result == null)
            return default;

        var body = result.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var item = JsonSerializer.Deserialize<T>(message);

        return item;
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

    // 생성
    private async Task<(IConnection, IChannel)> CreateAsync(RabbitConfig config)
    {
        // Create connection factory with configuration
        var factory = new ConnectionFactory
        {
            HostName = config.Host,
            Port = config.Port,
            UserName = config.Username,
            Password = config.Password,
            VirtualHost = config.VirtualHost,
            Ssl = {
                Enabled = config.SslEnabled
            }
        };

        // Establish connection and channel asynchronously
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        // Declare queue with retry and poison queue configurations
        var arguments = new Dictionary<string, object>
        {
            { "x-message-ttl", config.MessageTTLSecs * 1000 }, // Convert to milliseconds
            { "x-dead-letter-exchange", "" },
            { "x-dead-letter-routing-key", $"{config.QueueName}{config.PoisonQueueSuffix}" }
        };

        // Declare main queue
        await channel.QueueDeclareAsync(
            queue: config.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: arguments!
        );

        // Declare poison queue
        await channel.QueueDeclareAsync(
            queue: $"{config.QueueName}{config.PoisonQueueSuffix}",
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        // Set prefetch count to control message flow
        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: config.PrefetchCount,
            global: false
        );

        return (connection, channel);
    }
}