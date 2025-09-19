using IronHive.Abstractions.Queue;

namespace IronHive.Core.Storages;

/// <summary>
/// Json 직렬/역직렬화를 위한 페이로드 객체
/// </summary>
public class LocalQueuePayload<T>
{
    /// <summary> 메시지의 고유 식별자입니다. </summary>
    public required string Id { get; set; }

    /// <summary> 메시지의 본문입니다. </summary>
    public required T Body { get; set; }

    /// <summary> 메시지가 큐에 들어간 시점입니다. </summary>
    public required DateTimeOffset EnqueuedAt { get; set; }

    /// <summary> 메시지의 만료 시점입니다. (만료되지 않는 경우 null) </summary>
    public DateTimeOffset? ExpiresAt { get; set; }
}

/// <inheritdoc />
public class LocalQueueMessage<T> : LocalQueuePayload<T>, IQueueMessage<T>
{
    private readonly string _filePath;

    public LocalQueueMessage(string filePath)
    {
        _filePath = filePath;
    }

    /// <inheritdoc />
    public Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        EnsureValidFile(_filePath);

        // 확인된 메시지 파일을 삭제합니다.
        File.Delete(_filePath);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RequeueAsync(CancellationToken cancellationToken = default)
    {
        EnsureValidFile(_filePath);

        var directoryPath = Path.GetDirectoryName(_filePath)
            ?? throw new InvalidOperationException("Failed to get directory path from file path.");
        var newFileName = $"{DateTimeOffset.UtcNow.Ticks:D19}_{Id}{LocalQueueStorage.MessageExtension}";
        var newFilePath = Path.Combine(directoryPath, newFileName);

        try
        {
            // .qlock → 새 .qmsg (원자적 이동, 내용/TTL 보존)
            File.Move(_filePath, newFilePath);
        }
        catch (IOException)
        {
            // 이동 실패 시 lock 상태에 남겨둡니다.
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeadAsync(string reason, CancellationToken cancellationToken = default)
    {
        EnsureValidFile(_filePath);

        try
        {
            var deadFilePath = Path.ChangeExtension(_filePath, LocalQueueStorage.DeadMessageExtension);
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(_filePath, deadFilePath);
            File.WriteAllText(deadFilePath + ".reason", $"Time:{DateTimeOffset.UtcNow:o}\n Reason:{reason}");
        }
        catch (IOException)
        {
            // 이동 실패 시 lock 상태에 남겨둡니다.
        }

        return Task.CompletedTask;
    }

    /// <summary>파일이 유효한지 확인합니다.</summary>
    private static void EnsureValidFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File '{filePath}' does not exist.", filePath);

        if (Path.GetExtension(filePath) != LocalQueueStorage.LockExtension)
            throw new ArgumentException($"File '{filePath}' is not a locked queue message file.", nameof(filePath));
    }
}
