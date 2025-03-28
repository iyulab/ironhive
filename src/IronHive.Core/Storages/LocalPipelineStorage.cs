using IronHive.Abstractions.Memory;
using System.Collections.Concurrent;

namespace IronHive.Core.Storages;

public class LocalPipelineStorage : PipelineStorageBase
{
    // 스레드 세이프 컬렉션을 사용합니다.
    private readonly ConcurrentDictionary<string, object?> _storage = new();

    /// <inheritdoc />
    public override void Dispose()
    {
        _storage.Clear();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public override Task<bool> ContainsKeyAsync(
        string key, 
        CancellationToken cancellationToken = default)
    {
        var isExists = _storage.ContainsKey(key);
        return Task.FromResult(isExists);
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<string>> GetKeysAsync(
        CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_storage.Keys);
    }

    /// <inheritdoc />
    public override Task<T> GetValueAsync<T>(
        string key, 
        CancellationToken cancellationToken = default)
    {
        if (_storage.TryGetValue(key, out var obj))
        {
            if (obj is T t)
            {
                return Task.FromResult(t);
            }
            else if (obj is byte[] bytes)
            {
                var value = Deserialize<T>(bytes, cancellationToken);
                return Task.FromResult(value);
            }
        }

        return Task.FromResult(default(T)!);
    }

    /// <inheritdoc />
    public override Task SetValueAsync<T>(
        string key, 
        T value, 
        CancellationToken cancellationToken = default)
    {
        if (_storage.ContainsKey(key))
            throw new InvalidOperationException($"Key '{key}' already exists.");
        _storage[key] = Serialize(value, cancellationToken);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task DeleteKeyAsync(
        string key, 
        CancellationToken cancellationToken = default)
    {
        _storage.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}
