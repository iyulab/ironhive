using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using IronHive.Abstractions.Memory;
using System.Net.Http.Headers;
using RabbitMQ.Client.Exceptions;

namespace IronHive.Storages.RabbitMQ;

public class RabbitMQueueStorage : IQueueStorage
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly RabbitMQConfig _config;

    public RabbitMQueueStorage(RabbitMQConfig config)
    {
        _config = config;
        (_connection, _channel) = CreateConnection(config);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// RabbitMQ는 기본적으로 큐목록을 조회할 수 있는 기능을 제공하지 않으므로,
    /// Management Plugin을 사용하여 큐 목록을 조회, Management Port가 열려 있어야 함
    /// </summary>
    public async Task<IEnumerable<string>> ListQueuesAsync(CancellationToken cancellationToken = default)
    {
        // Management Plugin이 활성화되어 있어야 함
        // Management Port가 열려 있어야 함
        using var client = new HttpClient();
        var bytes = Encoding.ASCII.GetBytes($"{_config.Username}:{_config.Password}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(bytes));   
        var url = $"http://{_config.Host}:{_config.ManagementPort}/api/queues";
        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var jsonDoc = JsonDocument.Parse(content);
        var names = jsonDoc.RootElement.EnumerateArray()
                       .Select(element => element.GetProperty("name").GetString()!)
                       .ToList();

        return names ?? Enumerable.Empty<string>();
    }

    /// <inheritdoc />
    public async Task<bool> ExistsQueueAsync(
        string name, 
        CancellationToken cancellationToken = default)
    {
        var queues = await ListQueuesAsync(cancellationToken);
        return queues.Contains(name);
    }

    /// <inheritdoc />
    public async Task CreateQueueAsync(
        string name, 
        CancellationToken cancellationToken = default)
    {
        await _channel.QueueDeclareAsync(
            queue: name,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                { "x-message-ttl", _config.MessageTTLSecs * 1000 } // 밀리초 단위로 변환
            },
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task DeleteQueueAsync(
        string name, 
        CancellationToken cancellationToken = default)
    {
        await _channel.QueueDeleteAsync(
            queue: name,
            ifUnused: false,
            ifEmpty: false,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task EnqueueAsync<T>(
        string name,
        T item, 
        CancellationToken cancellationToken = default)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        var body = item.Serialize();

        await _channel.BasicPublishAsync(
            exchange: "",
            routingKey: name,
            mandatory: false,
            body: body,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<T?> DequeueAsync<T>(
        string name,
        CancellationToken cancellationToken = default)
    {
        var result = await _channel.BasicGetAsync(name, autoAck: false, cancellationToken);
        if (result == null)
            return default;

        try
        {
            var body = result.Body.ToArray();
            var item = body.Deserialize<T>();

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
    public async Task<int> CountAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var count = await _channel.MessageCountAsync(name, cancellationToken);
        return (int)count;
    }

    /// <inheritdoc />
    public async Task ClearAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        await _channel.QueuePurgeAsync(name, cancellationToken);
    }

    // RabbitMQ 연결 및 채널 생성
    private (IConnection, IChannel) CreateConnection(RabbitMQConfig config)
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

        // 동기로 연결 및 채널 생성
        var connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        var channel = connection.CreateChannelAsync().GetAwaiter().GetResult();

        return (connection, channel);
    }
}
