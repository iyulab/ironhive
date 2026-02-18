using IronHive.Abstractions.Streaming;

namespace IronHive.Core.Streaming;

/// <summary>
/// 스트림 상태의 기본 구현체입니다.
/// </summary>
public sealed class StreamState : IStreamState
{
    private readonly object _lock = new();
    private readonly StringBuilder _contentBuilder = new();
    private readonly Dictionary<string, object> _metadata = [];

    /// <inheritdoc />
    public string StreamId { get; }

    /// <inheritdoc />
    public DateTime CreatedAt { get; }

    /// <inheritdoc />
    public DateTime LastUpdatedAt { get; private set; }

    /// <inheritdoc />
    public StreamStatus Status { get; private set; }

    /// <inheritdoc />
    public int LastChunkIndex { get; private set; }

    /// <inheritdoc />
    public int TotalChunksReceived { get; private set; }

    /// <inheritdoc />
    public string AccumulatedContent
    {
        get
        {
            lock (_lock)
            {
                return _contentBuilder.ToString();
            }
        }
    }

    /// <inheritdoc />
    public bool CanResume => Status is StreamStatus.Paused or StreamStatus.Disconnected;

    /// <inheritdoc />
    public string? ErrorMessage { get; private set; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object>? Metadata => _metadata.AsReadOnly();

    public StreamState(string? streamId = null, IDictionary<string, object>? metadata = null)
    {
        StreamId = streamId ?? Guid.NewGuid().ToString("N");
        CreatedAt = DateTime.UtcNow;
        LastUpdatedAt = CreatedAt;
        Status = StreamStatus.Pending;

        if (metadata != null)
        {
            foreach (var kvp in metadata)
            {
                _metadata[kvp.Key] = kvp.Value;
            }
        }
    }

    /// <summary>
    /// 청크를 추가합니다.
    /// </summary>
    public void AppendChunk(string content, int chunkIndex)
    {
        lock (_lock)
        {
            if (Status == StreamStatus.Pending)
            {
                Status = StreamStatus.Streaming;
            }

            _contentBuilder.Append(content);
            LastChunkIndex = chunkIndex;
            TotalChunksReceived++;
            LastUpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// 상태를 업데이트합니다.
    /// </summary>
    public void UpdateStatus(StreamStatus status, string? errorMessage = null)
    {
        lock (_lock)
        {
            Status = status;
            ErrorMessage = errorMessage;
            LastUpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// 메타데이터를 설정합니다.
    /// </summary>
    public void SetMetadata(string key, object value)
    {
        lock (_lock)
        {
            _metadata[key] = value;
            LastUpdatedAt = DateTime.UtcNow;
        }
    }
}

internal sealed class StringBuilder
{
    private readonly System.Text.StringBuilder _sb = new();

    public void Append(string? value) => _sb.Append(value);

    public override string ToString() => _sb.ToString();
}
