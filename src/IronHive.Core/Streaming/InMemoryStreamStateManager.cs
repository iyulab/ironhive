using System.Collections.Concurrent;
using IronHive.Abstractions.Streaming;

namespace IronHive.Core.Streaming;

/// <summary>
/// 메모리 기반 스트림 상태 관리자입니다.
/// 단일 프로세스 환경에 적합합니다.
/// </summary>
public sealed class InMemoryStreamStateManager : IStreamStateManager, IDisposable
{
    private readonly ConcurrentDictionary<string, StreamState> _states = new();
    private readonly StreamStateOptions _options;
    private readonly Timer? _cleanupTimer;
    private bool _disposed;

    public InMemoryStreamStateManager(StreamStateOptions? options = null)
    {
        _options = options ?? new StreamStateOptions();

        if (_options.EnableAutoCleanup)
        {
            _cleanupTimer = new Timer(
                _ => _ = CleanupExpiredAsync(_options.ExpirationTime, CancellationToken.None),
                null,
                _options.CleanupInterval,
                _options.CleanupInterval);
        }
    }

    /// <inheritdoc />
    public Task<IStreamState> CreateStateAsync(
        string? streamId = null,
        IDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var state = new StreamState(streamId, metadata);

        if (!_states.TryAdd(state.StreamId, state))
        {
            throw new InvalidOperationException($"Stream with ID '{state.StreamId}' already exists.");
        }

        return Task.FromResult<IStreamState>(state);
    }

    /// <inheritdoc />
    public Task<IStreamState?> GetStateAsync(
        string streamId,
        CancellationToken cancellationToken = default)
    {
        _states.TryGetValue(streamId, out var state);
        return Task.FromResult<IStreamState?>(state);
    }

    /// <inheritdoc />
    public Task AppendChunkAsync(
        string streamId,
        string chunkContent,
        int chunkIndex,
        CancellationToken cancellationToken = default)
    {
        if (!_states.TryGetValue(streamId, out var state))
        {
            throw new KeyNotFoundException($"Stream with ID '{streamId}' not found.");
        }

        if (state.TotalChunksReceived >= _options.MaxChunksPerStream)
        {
            throw new InvalidOperationException(
                $"Maximum chunks ({_options.MaxChunksPerStream}) exceeded for stream '{streamId}'.");
        }

        state.AppendChunk(chunkContent, chunkIndex);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateStatusAsync(
        string streamId,
        StreamStatus status,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        if (!_states.TryGetValue(streamId, out var state))
        {
            throw new KeyNotFoundException($"Stream with ID '{streamId}' not found.");
        }

        state.UpdateStatus(status, errorMessage);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task MarkAsDisconnectedAsync(
        string streamId,
        CancellationToken cancellationToken = default)
    {
        if (!_states.TryGetValue(streamId, out var state))
        {
            throw new KeyNotFoundException($"Stream with ID '{streamId}' not found.");
        }

        state.UpdateStatus(StreamStatus.Disconnected);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteStateAsync(
        string streamId,
        CancellationToken cancellationToken = default)
    {
        _states.TryRemove(streamId, out _);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<int> CleanupExpiredAsync(
        TimeSpan expirationTime,
        CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTime.UtcNow - expirationTime;
        var expiredIds = new List<string>();

        foreach (var kvp in _states)
        {
            var state = kvp.Value;

            // 완료되었거나 만료된 스트림
            if (state.Status == StreamStatus.Completed ||
                state.Status == StreamStatus.Failed ||
                state.Status == StreamStatus.Cancelled)
            {
                if (state.LastUpdatedAt < cutoffTime)
                {
                    expiredIds.Add(kvp.Key);
                }
            }
            // 재개 가능 윈도우가 지난 스트림
            else if (state.Status == StreamStatus.Disconnected ||
                     state.Status == StreamStatus.Paused)
            {
                var resumeWindowCutoff = DateTime.UtcNow - _options.ResumeWindow;
                if (state.LastUpdatedAt < resumeWindowCutoff)
                {
                    expiredIds.Add(kvp.Key);
                    state.UpdateStatus(StreamStatus.Expired);
                }
            }
        }

        foreach (var id in expiredIds)
        {
            _states.TryRemove(id, out _);
        }

        return Task.FromResult(expiredIds.Count);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> GetResumableStreamsAsync(
        CancellationToken cancellationToken = default)
    {
        var resumable = _states
            .Where(kvp => kvp.Value.CanResume)
            .Select(kvp => kvp.Key)
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(resumable);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _cleanupTimer?.Dispose();
        _states.Clear();
    }
}
