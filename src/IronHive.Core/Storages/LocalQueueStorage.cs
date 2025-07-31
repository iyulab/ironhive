using IronHive.Abstractions.Storages;
using System.Collections.Concurrent;
using System.Text.Json;

namespace IronHive.Core.Storages;

/// <summary>
/// 로컬 파일시스템을 이용한 큐 스토리지 구현
/// </summary>
public class LocalQueueStorage<T> : IQueueStorage<T>
{
    private const string MessageExtension = "qmsg";
    private const string LockExtension = "qlock";
    private const string DeadMessageExtension = "qdead";
    private const int CacheQueueSize = 100;

    private readonly ConcurrentQueue<string> _cache = new();
    private readonly string _directoryPath;
    private readonly TimeSpan? _ttl;
    private readonly JsonSerializerOptions _jsonOptions;

    public LocalQueueStorage(LocalQueueConfig config)
    {
        _directoryPath = config.DirectoryPath;
        _ttl = config.TimeToLive;
        _jsonOptions = config.JsonOptions;

        Directory.CreateDirectory(_directoryPath);
        RestoreQueueMessages();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task EnqueueAsync(T message, CancellationToken cancellationToken = default)
    {
        // 현재 UTC 시간을 기준으로 enqueueTicks를 생성합니다.
        var enqueueTicks = DateTime.UtcNow.Ticks;
        // TTL이 설정된 경우, 만료 시간을 계산합니다. 설정 되지 않은 경우 0으로 설정합니다.
        var expirationTicks = _ttl.HasValue ? DateTime.UtcNow.Add(_ttl.Value).Ticks : 0;

        var fileName = $"{enqueueTicks}_{expirationTicks}.{MessageExtension}";
        var filePath = Path.Combine(_directoryPath, fileName);

        // 메시지 직렬화 후 파일에 저장
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(message, _jsonOptions);
        await File.WriteAllBytesAsync(filePath, bytes, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TaggedMessage<T>?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        // 캐시가 비어있으면 디렉토리에서 파일을 가져와서 캐시에 추가합니다.
        if (_cache.IsEmpty)
        {
            foreach(var file in Directory.EnumerateFiles(_directoryPath, $"*.{MessageExtension}").OrderBy(f => f).Take(CacheQueueSize))
            {
                _cache.Enqueue(file);
            }
        }
        
        while(_cache.TryDequeue(out var queueFilePath) || cancellationToken.IsCancellationRequested)
        {
            if (string.IsNullOrEmpty(queueFilePath))
                continue;

            var lockedFilePath = Path.ChangeExtension(queueFilePath, $".{LockExtension}");
            try
            {
                // 큐 파일이 존재하지 않으면 건너뜁니다.
                if (!File.Exists(queueFilePath))
                    continue;

                // 메시지 파일을 잠금상태로 변경합니다.
                File.Move(queueFilePath, lockedFilePath);

                // (파일명: "{enqueueTicks}_{expirationInfo}.lock")
                var parts = Path.GetFileNameWithoutExtension(lockedFilePath).Split('_');
                // 파일명이 잘못된 경우 삭제합니다.
                if (parts.Length != 2)
                {
                    File.Delete(lockedFilePath);
                    continue;
                }
                // TTL이 만료된 파일인 경우 삭제합니다.
                if (long.TryParse(parts[1], out long expirationTicks))
                {
                    if (expirationTicks != 0 && (DateTime.UtcNow.Ticks > expirationTicks))
                    {
                        File.Delete(lockedFilePath);
                        continue;
                    }
                }
                // 만료 정보가 잘못된 파일인 경우(숫자X) 삭제합니다.
                else
                {
                    File.Delete(lockedFilePath);
                    continue;
                }

                var bytes = await File.ReadAllBytesAsync(lockedFilePath, cancellationToken);
                var message = JsonSerializer.Deserialize<T>(bytes, _jsonOptions)
                    ?? throw new JsonException("Failed to deserialize message from file.");

                return new TaggedMessage<T>
                {
                    Message = message,
                    AckTag = lockedFilePath
                };
            }
            // 취소 요청이 들어온 경우, 예외를 발생시켜 작업을 중단합니다.
            catch (OperationCanceledException)
            {
                throw;
            }
            // 다른 소비자가 이미 처리 중일 경우 건너뜁니다.
            catch (IOException)
            {
                continue;
            }
            // 이외의 경우 예외가 발생하면 해당 파일을 dead 메시지로 이동합니다.
            catch (Exception)
            {
                var deadFilePath = Path.ChangeExtension(lockedFilePath, $".{DeadMessageExtension}");
                try
                {
                    // 파일이 없는 경우 건너뜁니다.
                    if (!File.Exists(lockedFilePath))
                        continue; 

                    File.Move(lockedFilePath, deadFilePath);
                }
                catch (IOException)
                {
                    // 이동 실패 시, lock 상태에 남겨둡니다.
                    // 이 경우, 나중에 다시 처리할 수 있도록 유지합니다.
                }
            }
        }

        // 큐가 비어있으면 null 반환
        return null;
    }

    /// <inheritdoc />
    public Task AckAsync(object ackTag, CancellationToken cancellationToken = default)
    {
        // ackTag는 lock 상태 파일의 경로입니다.
        if (ackTag is not string lockedFilePath)
            throw new InvalidOperationException("Invalid ack tag.");

        if (File.Exists(lockedFilePath))
        {
            // 확인된 메시지의 lock 상태 파일을 삭제합니다.
            File.Delete(lockedFilePath);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task NackAsync(object ackTag, bool requeue = false, CancellationToken cancellationToken = default)
    {
        if (ackTag is not string lockedFilePath)
            throw new InvalidOperationException("Invalid ack tag.");

        // lock 상태 파일이 존재하지 않으면 아무 작업도 하지 않습니다.
        if (!File.Exists(lockedFilePath))
        {
            return Task.CompletedTask;
        }

        if (requeue)
        {
            // requeue가 true인 경우, lock 상태 파일을 원래 큐 폴더로 이동합니다.
            var originalPath = Path.ChangeExtension(lockedFilePath, $".{MessageExtension}");
            try
            {
                File.Move(lockedFilePath, originalPath);
            }
            catch (IOException)
            {
                // 이동 실패 시 lock 상태에 남겨둡니다.
            }
        }
        else
        {
            // requeue가 false인 경우, lock 상태 파일을 삭제합니다.
            File.Delete(lockedFilePath);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        var count = Directory.GetFiles(_directoryPath, $"*.{MessageExtension}").Length;
        return Task.FromResult(count);
    }

    /// <inheritdoc />
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        if (Directory.Exists(_directoryPath))
        {
            Directory.Delete(_directoryPath, recursive: true);
        }
        Directory.CreateDirectory(_directoryPath);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 시스템 시작 시 처리 중(lock 상태)의 메시지를 다시 큐에 복구합니다.
    /// </summary>
    private void RestoreQueueMessages()
    {
        var lockedFiles = Directory.GetFiles(_directoryPath, $"*.{LockExtension}");
        var deadFiles = Directory.GetFiles(_directoryPath, $"*.{DeadMessageExtension}");
        var allFiles = lockedFiles.Concat(deadFiles).ToList();

        foreach (var file in allFiles)
        {
            var msgFile = Path.ChangeExtension(file, $".{MessageExtension}");

            try
            {
                File.Move(file, msgFile, overwrite: false); // 이미 있을 경우 덮어쓰지 않음
            }
            catch (IOException)
            {
                // 동일 이름의 msg가 있을 경우 삭제
                if (File.Exists(msgFile))
                {
                    File.Delete(msgFile);
                }
            }
        }
    }
}
