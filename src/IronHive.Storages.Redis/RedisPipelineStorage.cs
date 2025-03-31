using IronHive.Abstractions.Memory;
using MessagePack.Resolvers;
using MessagePack;
using StackExchange.Redis;

namespace IronHive.Storages.Redis;

public class RedisPipelineStorage : IPipelineStorage
{
    private readonly ConnectionMultiplexer _connection;
    private readonly IDatabase _db;
    private readonly MessagePackSerializerOptions _options = ContractlessStandardResolver.Options
        .WithCompression(MessagePackCompression.Lz4Block);

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
    public async Task<IEnumerable<string>> GetKeysAsync(
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        prefix ??= string.Empty;
        var output = await _db.ExecuteAsync("KEYS", $"{prefix}*");
        var keys = ((string?[]?)output)?.ToList()
            .Where(k => !string.IsNullOrEmpty(k))
            .Select(k => k!)
            ?? Enumerable.Empty<string>();
        return keys;
    }

    /// <inheritdoc />
    public async Task<bool> ContainsKeyAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        return await _db.KeyExistsAsync(key);
    }

    /// <inheritdoc />
    public async Task<T> GetValueAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty)
            throw new KeyNotFoundException($"Value not found for key: {key}");
        var bytes = (byte[]?)value ?? Array.Empty<byte>();

        var result = MessagePackSerializer.Deserialize<T>(bytes, _options, cancellationToken);
        return result;
    }

    /// <inheritdoc />
    public async Task SetValueAsync<T>(
        string key,
        T value,
        CancellationToken cancellationToken = default)
    {
        var bytes = MessagePackSerializer.Serialize(value, _options, cancellationToken);
        await _db.StringSetAsync(key, bytes);
    }

    /// <inheritdoc />
    public async Task DeleteKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        await _db.KeyDeleteAsync(key);
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
