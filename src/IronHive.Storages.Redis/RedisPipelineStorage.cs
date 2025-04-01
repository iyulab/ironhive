using IronHive.Abstractions.Memory;
using StackExchange.Redis;

namespace IronHive.Storages.Redis;

public class RedisPipelineStorage : IPipelineStorage
{
    private readonly ConnectionMultiplexer _connection;
    private readonly IDatabase _db;

    public RedisPipelineStorage(RedisConfig config)
    {
        _connection = CreateConnection(config);
        _db = _connection.GetDatabase();
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DataPipeline>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var cr = await _db.ExecuteAsync("keys", "*");
        var keys = (string[]?)cr ?? Array.Empty<string>();

        var results = new List<DataPipeline>();
        foreach (var key in keys)
        {
            var value = await GetAsync(key, cancellationToken);
            results.Add(value);
        }
        return results;
    }

    /// <inheritdoc />
    public async Task<bool> ContainsAsync(
        string id, 
        CancellationToken cancellationToken = default)
    {
        return await _db.KeyExistsAsync(id);
    }

    /// <inheritdoc />
    public async Task<DataPipeline> GetAsync(
        string id, 
        CancellationToken cancellationToken = default)
    {
        var value = await _db.StringGetAsync(id);
        if (value.IsNullOrEmpty)
            throw new KeyNotFoundException($"Value not found for key: {id}");
        var bytes = (byte[]?)value ?? Array.Empty<byte>();

        var result = bytes.Deserialize<DataPipeline>()
            ?? throw new InvalidOperationException("Failed to deserialize pipeline.");
        return result;
    }

    /// <inheritdoc />
    public async Task SetAsync(
        DataPipeline pipeline, 
        CancellationToken cancellationToken = default)
    {
        var bytes = pipeline.Serialize();
        await _db.StringSetAsync(pipeline.Id, bytes);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        string id, 
        CancellationToken cancellationToken = default)
    {
        await _db.KeyDeleteAsync(id);
    }

    // Redis 연결을 생성합니다.
    private static ConnectionMultiplexer CreateConnection(RedisConfig config)
    {
        if (!string.IsNullOrWhiteSpace(config.ConnectionString))
        {
            return ConnectionMultiplexer.Connect(config.ConnectionString);
        }
        else
        {
            return ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                EndPoints = { $"{config.Host}:{config.Port}" },
                Password = config.Password,
                AbortOnConnectFail = config.AbortOnConnectFail,
                ConnectTimeout = config.ConnectTimeout,
                ConnectRetry = config.ConnectRetry,
                DefaultDatabase = config.DefaultDatabase,
                Ssl = config.Ssl,
                SyncTimeout = config.SyncTimeout,
                AsyncTimeout = config.AsyncTimeout,
                KeepAlive = config.KeepAlive,
                ClientName = config.ClientName
            });
        }
    }
}
