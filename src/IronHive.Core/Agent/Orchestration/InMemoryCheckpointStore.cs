using System.Collections.Concurrent;
using IronHive.Abstractions.Agent.Orchestration;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// 인메모리 체크포인트 저장소 구현입니다.
/// </summary>
public class InMemoryCheckpointStore : ICheckpointStore
{
    private readonly ConcurrentDictionary<string, OrchestrationCheckpoint> _store = new();

    /// <inheritdoc />
    public Task SaveAsync(string orchestrationId, OrchestrationCheckpoint checkpoint, CancellationToken ct = default)
    {
        _store[orchestrationId] = checkpoint;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<OrchestrationCheckpoint?> LoadAsync(string orchestrationId, CancellationToken ct = default)
    {
        _store.TryGetValue(orchestrationId, out var checkpoint);
        return Task.FromResult(checkpoint);
    }

    /// <inheritdoc />
    public Task DeleteAsync(string orchestrationId, CancellationToken ct = default)
    {
        _store.TryRemove(orchestrationId, out _);
        return Task.CompletedTask;
    }
}
