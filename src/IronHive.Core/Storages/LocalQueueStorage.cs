using System.Text.Json;
using System.Collections.Concurrent;
using IronHive.Abstractions.Queue;

namespace IronHive.Core.Storages;

/// <summary>
/// 로컬 파일시스템을 이용한 큐 스토리지 구현
/// </summary>
public class LocalQueueStorage : IQueueStorage
{
    public const string MessageExtension = ".qmsg";
    public const string LockExtension = ".qlock";
    public const string DeadMessageExtension = ".qdead";

    private readonly TimeSpan? _messageTtl;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ConcurrentQueue<string> _cache = new();
    private readonly int _cacheSize;

    public LocalQueueStorage(string directoryPath)
        : this(new LocalQueueConfig { DirectoryPath = directoryPath })
    { }

    public LocalQueueStorage(LocalQueueConfig config)
    {
        _messageTtl = config.TimeToLive;
        _jsonOptions = config.JsonOptions;

        if (config.CacheSize < 1)
            throw new ArgumentOutOfRangeException(nameof(config), "Cache size must be greater than zero.");
        _cacheSize = config.CacheSize;

        DirectoryPath = config.DirectoryPath;
        Directory.CreateDirectory(DirectoryPath);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cache.Clear();
        GC.SuppressFinalize(this);
    }

    /// <summary> 큐 파일이 저장될 디렉토리 경로 </summary>
    public string DirectoryPath { get; }

    /// <summary>
    /// 큐에 처리되지 않은 메시지(lock/dead)들을 다시 복원후, 복원된 메시지들의 본문을 반환합니다.
    /// </summary>
    public async Task<IEnumerable<T>> RestoreAsync<T>()
    {
        var targets = new List<string>();
        targets.AddRange(Directory.GetFiles(DirectoryPath, $"*"));
        //targets.AddRange(Directory.GetFiles(DirectoryPath, $"*{LockExtension}"));
        //targets.AddRange(Directory.GetFiles(DirectoryPath, $"*{DeadMessageExtension}"));

        var messages = new List<T>();
        foreach (var filePath in targets)
        {
            try
            {
                var queueFilePath = filePath;
                // lock/dead 파일의 경우 일반 메시지 파일로 변경
                if (filePath.EndsWith(LockExtension, StringComparison.OrdinalIgnoreCase) ||
                    filePath.EndsWith(DeadMessageExtension, StringComparison.OrdinalIgnoreCase))
                {
                    queueFilePath = Path.ChangeExtension(filePath, MessageExtension);
                    File.Move(filePath, queueFilePath, overwrite: false);
                }
                
                var bytes = await File.ReadAllBytesAsync(queueFilePath);
                var payload = JsonSerializer.Deserialize<LocalQueuePayload<T>>(bytes, _jsonOptions);
                if (payload == null)
                    continue;

                messages.Add(payload.Body);
            }
            catch (IOException)
            {
                // 충돌/접근 문제 발생 무시
            }
        }
        return messages;
    }

    /// <inheritdoc />
    public async Task<IQueueConsumer> CreateConsumerAsync<T>(
        Func<IQueueMessage<T>, Task> onReceived, 
        CancellationToken cancellationToken = default)
    {
        var consumer = new LocalQueueConsumer<T>(this)
        {
            OnReceived = onReceived
        };

        cancellationToken.ThrowIfCancellationRequested();
        await Task.CompletedTask;
        return consumer;
    }

    /// <inheritdoc />
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        var count = Directory.GetFiles(DirectoryPath, $"*{MessageExtension}").Length;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(count);
    }

    /// <inheritdoc />
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        if (Directory.Exists(DirectoryPath))
        {
            cancellationToken.ThrowIfCancellationRequested();
            Directory.Delete(DirectoryPath, recursive: true);
        }
        Directory.CreateDirectory(DirectoryPath);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task EnqueueAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        // 파일명은 ticks 19자리 기준으로 생성하여 정렬시 오래된 순서대로 처리되도록 합니다.
        var now = DateTimeOffset.UtcNow;
        var id = Guid.NewGuid().ToString("N");
        var fileName = $"{now.Ticks:D19}_{id}{MessageExtension}";

        // 메시지 직렬화 후 파일에 저장 (동시성 문제로 인해 임시파일로 저장 후 이동)
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(new LocalQueuePayload<T>
        {
            Id = id,
            Body = message,
            EnqueuedAt = now,
            ExpiresAt = _messageTtl.HasValue ? now.Add(_messageTtl.Value) : null
        }, _jsonOptions);

        cancellationToken.ThrowIfCancellationRequested();
        var tmpPath = Path.Combine(DirectoryPath, $"{fileName}.tmp");
        var filePath = Path.Combine(DirectoryPath, fileName);
        await File.WriteAllBytesAsync(tmpPath, bytes, cancellationToken);
        try
        {
            File.Move(tmpPath, filePath, overwrite: false);
        }
        catch
        {
            try { if (File.Exists(tmpPath)) File.Delete(tmpPath); } catch { /* swallow */ }
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IQueueMessage<T>?> DequeueAsync<T>(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // 캐시가 비어있으면 null 반환
            if (!_cache.TryDequeue(out var queueFilePath))
            {
                // 캐시가 비어있고 채우지 못하면 null 반환
                if (_cache.IsEmpty && !TryFillCache())
                    return null;
                // 캐시에서 다시 시도
                if (!_cache.TryDequeue(out queueFilePath))
                    return null;
            }
            if (string.IsNullOrWhiteSpace(queueFilePath))
                return null;

            var lockedFilePath = Path.ChangeExtension(queueFilePath, LockExtension);
            try
            {
                // 큐 파일이 존재하지 않으면 건너뜁니다.
                if (!File.Exists(queueFilePath))
                    continue;

                // 메시지 파일을 잠금상태로 변경합니다.
                File.Move(queueFilePath, lockedFilePath);

                var bytes = await File.ReadAllBytesAsync(lockedFilePath, cancellationToken);
                var payload = JsonSerializer.Deserialize<LocalQueuePayload<T>>(bytes, _jsonOptions)
                    ?? throw new JsonException("Failed to deserialize message from queue file.");

                // 메시지가 만료된 경우 파일을 삭제하고 다음 파일로 넘어갑니다.
                if (payload.ExpiresAt.HasValue && payload.ExpiresAt.Value <= DateTimeOffset.UtcNow)
                {
                    File.Delete(lockedFilePath);
                    continue;
                }

                return new LocalQueueMessage<T>(lockedFilePath)
                {
                    Id = payload.Id,
                    Body = payload.Body,
                    EnqueuedAt = payload.EnqueuedAt,
                    ExpiresAt = payload.ExpiresAt,
                };
            }
            // 다른 소비자가 처리 중일 경우 건너뜁니다.
            catch (IOException)
            {
                continue;
            }
            // 이외의 경우 예외가 발생하면 해당 파일을 dead 메시지로 이동합니다.
            catch (Exception ex)
            {
                try
                {
                    // 파일이 없는 경우 건너뜁니다.
                    if (!File.Exists(lockedFilePath))
                        continue;

                    var deadFilePath = Path.ChangeExtension(lockedFilePath, DeadMessageExtension);
                    File.Move(lockedFilePath, deadFilePath);
                    File.WriteAllText(deadFilePath + ".reason", $"Time:{DateTimeOffset.UtcNow:o}\nReason:{ex}");
                }
                catch (IOException)
                {
                    // 이동 실패 시, lock 상태에 남겨둡니다.
                }

                // 취소 요청의 경우, 예외를 전파합니다
                if (ex is OperationCanceledException)
                {
                    throw;
                }
            }
        }

        throw new OperationCanceledException("Dequeue operation was canceled.", cancellationToken);
    }

    /// <summary> 캐시를 채웁니다. </summary>
    /// <returns>캐시가 비어있지 않으면 true</returns>
    private bool TryFillCache()
    {
        foreach (var f in Directory.EnumerateFiles(DirectoryPath, $"*{MessageExtension}")
            .OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal)
            .Take(_cacheSize))
        {
            _cache.Enqueue(f);
        }
        return !_cache.IsEmpty;
    }
}
