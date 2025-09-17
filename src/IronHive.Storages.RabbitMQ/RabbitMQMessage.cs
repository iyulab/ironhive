using IronHive.Abstractions.Queue;
using RabbitMQ.Client;

namespace IronHive.Storages.RabbitMQ;

/// <inheritdoc />
public class RabbitMQMessage<T> : IQueueMessage<T>
{
    private readonly IChannel _channel;

    public RabbitMQMessage(IChannel channel)
    {
        _channel = channel;
    }

    /// <summary>
    /// 처리중인 메시지의 고유 식별자입니다.
    /// </summary>
    public required ulong DeliveryTag { get; init; }

    /// <inheritdoc />
    public required T Body { get; set; }

    /// <inheritdoc />
    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        await _channel.BasicAckAsync(DeliveryTag, false, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RequeueAsync(CancellationToken cancellationToken = default)
    {
        await _channel.BasicNackAsync(DeliveryTag, false, true, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeadAsync(string reason, CancellationToken cancellationToken = default)
    {
        await _channel.BasicNackAsync(DeliveryTag, false, false, cancellationToken);
    }
}
