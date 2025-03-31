using IronHive.Abstractions.Memory;
using System.Collections.Concurrent;

namespace IronHive.Core.Storages;

public class LocalPipelineStorage : IPipelineStorage
{
    // 스레드 세이프 컬렉션을 사용합니다.
    private readonly ConcurrentDictionary<string, object?> _dic = new();

    /// <inheritdoc />
    public void Dispose()
    {
        _dic.Clear();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public Task<IEnumerable<string>> GetKeysAsync(
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        prefix ??= string.Empty;
        var keys = _dic.Keys.Where(k => k.StartsWith(prefix)).AsEnumerable();
        return Task.FromResult(keys);
    }

    /// <inheritdoc />
    public Task<bool> ContainsKeyAsync(
        string key, 
        CancellationToken cancellationToken = default)
    {
        var isExists = _dic.ContainsKey(key);
        return Task.FromResult(isExists);
    }

    /// <inheritdoc />
    public Task<T> GetValueAsync<T>(
        string key, 
        CancellationToken cancellationToken = default)
    {
        if (_dic.TryGetValue(key, out var obj))
        {
            return Task.FromResult((T)obj!);
        }

        return Task.FromResult(default(T)!);
    }

    /// <inheritdoc />
    public Task SetValueAsync<T>(
        string key, 
        T value, 
        CancellationToken cancellationToken = default)
    {
        if (_dic.ContainsKey(key))
            throw new InvalidOperationException($"Key '{key}' already exists.");
        _dic[key] = value;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteKeyAsync(
        string key, 
        CancellationToken cancellationToken = default)
    {
        _dic.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}
