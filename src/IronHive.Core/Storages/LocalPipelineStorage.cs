using IronHive.Abstractions.Memory;
using System.Collections.Concurrent;

namespace IronHive.Core.Storages;

public class LocalPipelineStorage : IPipelineStorage
{
    // 스레드 세이프 컬렉션을 사용합니다.
    private readonly ConcurrentDictionary<string, DataPipeline> _dic = new();

    /// <inheritdoc />
    public void Dispose()
    {
        _dic.Clear();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public Task<IEnumerable<DataPipeline>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var pipelines = _dic.Values.AsEnumerable();

        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(pipelines);
    }

    /// <inheritdoc />
    public Task<bool> ContainsAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var isExists = _dic.ContainsKey(id);
        
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(isExists);
    }

    /// <inheritdoc />
    public Task<DataPipeline> GetAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (!_dic.TryGetValue(id, out var pipeline))
            throw new KeyNotFoundException($"Key '{id}' not found.");
        
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(pipeline);
    }

    /// <inheritdoc />
    public Task SetAsync(
        DataPipeline pipeline,
        CancellationToken cancellationToken = default)
    {
        _dic[pipeline.Id] = pipeline;    

        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteAsync(
        string id, 
        CancellationToken cancellationToken = default)
    {
        _dic.TryRemove(id, out _);

        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
