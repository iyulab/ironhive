using IronHive.Abstractions.Memory;
using StackExchange.Redis;

namespace IronHive.Storages.Redis;

public class RedisPipelineStorage : PipelineStorageBase
{
    private readonly string _keyPrefix = "pipeline:";
    private readonly ConnectionMultiplexer _connection;
    private readonly IDatabase _db;

    public RedisPipelineStorage(RedisConfig config)
    {
        _connection = CreateConnection(config);
        _db = _connection.GetDatabase();
    }

    public override void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public override async Task<bool> ContainsKeyAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var rKey = CreateKey(key);
        return await _db.KeyExistsAsync(rKey);
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<string>> GetKeysAsync(
        CancellationToken cancellationToken = default)
    {
        var output = await _db.ExecuteAsync("KEYS", $"{_keyPrefix}*");
        var keys = ((string?[]?)output)?.ToArray()
            .Select(x => x!.Substring(_keyPrefix.Length))
            ?? Array.Empty<string>();
        return keys;
    }

    /// <inheritdoc />
    public override async Task<T> GetValueAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        var rKey = CreateKey(key);
        var value = await _db.StringGetAsync(rKey);
        if (value.IsNullOrEmpty)
            throw new KeyNotFoundException($"Value not found for key: {key}");
        var bytes = (byte[]?)value ?? Array.Empty<byte>();
        return Deserialize<T>(bytes, cancellationToken);
    }

    /// <inheritdoc />
    public override async Task SetValueAsync<T>(
        string key,
        T value,
        CancellationToken cancellationToken = default)
    {
        var rKey = CreateKey(key);
        var bytes = Serialize(value, cancellationToken);
        await _db.StringSetAsync(rKey, bytes);
    }

    /// <inheritdoc />
    public override async Task DeleteKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var rKey = CreateKey(key);
        await _db.KeyDeleteAsync(rKey);
    }

    private RedisKey CreateKey(string key)
    {
        return new(_keyPrefix + key);
    }

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
