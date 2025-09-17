using IronHive.Abstractions.Queue;
using RabbitMQ.Client;

namespace IronHive.Storages.RabbitMQ;

/// <inheritdoc />
public class RabbitMQConsumer : IQueueConsumer
{
    private readonly IAsyncBasicConsumer _consumer;   
    private string? _consumerTag;

    public RabbitMQConsumer(IAsyncBasicConsumer consumer)
    {
        _consumer = consumer;
    }

    /// <summary>
    /// 소비자가 메시지를 수신할 큐 이름입니다.
    /// </summary>
    public required string QueueName { get; init; }

    /// <inheritdoc />
    public bool IsRunning => string.IsNullOrEmpty(_consumerTag) == false;

    /// <inheritdoc />
    public void Dispose()
    {
        if (IsRunning)
            throw new InvalidOperationException("Cannot dispose a running consumer. Please stop it first.");

        _consumerTag = null;
        _consumer.Channel?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (IsRunning) return;
        if (_consumer.Channel == null || _consumer.Channel.IsClosed)
            throw new InvalidOperationException("The consumer's channel is closed.");

        _consumerTag = await _consumer.Channel.BasicConsumeAsync(
            queue: QueueName,
            consumer: _consumer,
            autoAck: false,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!IsRunning) return;
        if (_consumer.Channel == null || _consumer.Channel.IsClosed)
            throw new InvalidOperationException("The consumer's channel is closed.");
        if (string.IsNullOrEmpty(_consumerTag))
            throw new InvalidOperationException("The consumer is not started.");

        var tag = _consumerTag;
        _consumerTag = null;

        await _consumer.Channel.BasicCancelAsync(
            consumerTag: tag,
            noWait: false,
            cancellationToken: cancellationToken);
    }
}
