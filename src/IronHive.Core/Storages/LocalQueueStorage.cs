using System.Text.Json;
using System.Collections.Concurrent;
using IronHive.Abstractions.Queue;

namespace IronHive.Core.Storages;

/// <summary>
/// 로컬 파일시스템을 이용한 큐 스토리지 구현
/// </summary>
public class LocalQueueStorage : IQueueStorage
{
    private const string MessageExtension = "qmsg";
    private const string LockExtension = "qlock";
    private const string DeadMessageExtension = "qdead";

    private readonly string _directoryPath;
    private readonly TimeSpan? _ttl;
    private readonly int _cacheSize;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ConcurrentQueue<string> _cache = new();

    public LocalQueueStorage(LocalQueueConfig config)
    {
        _directoryPath = config.DirectoryPath;
        _ttl = config.TimeToLive;
        _cacheSize = config.CacheQueueSize > 0 ? config.CacheQueueSize : 100;
        _jsonOptions = config.JsonOptions;

        Directory.CreateDirectory(_directoryPath);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cache.Clear();
        GC.SuppressFinalize(this);
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

    /// <inheritdoc />
    public async Task EnqueueAsync<T>(T message, CancellationToken cancellationToken = default)
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
    public async Task<QueueMessage<T>?> DequeueAsync<T>(CancellationToken cancellationToken = default)
    {
        // 캐시가 비어있으면 디렉토리에서 파일을 가져와서 캐시에 추가합니다.
        if (_cache.IsEmpty)
        {
            foreach(var file in Directory.EnumerateFiles(_directoryPath, $"*.{MessageExtension}")
                .OrderBy(f => f).Take(_cacheSize))
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

                // 파일이 유효하지 않거나 만료된 경우, 해당 파일을 삭제하고 다음 파일로 넘어갑니다.
                if (IsInvalidOrExpired(lockedFilePath))
                {
                    File.Delete(lockedFilePath);
                    continue;
                }

                var bytes = await File.ReadAllBytesAsync(lockedFilePath, cancellationToken);
                var message = JsonSerializer.Deserialize<T>(bytes, _jsonOptions)
                    ?? throw new JsonException("Failed to deserialize message from file.");

                return new QueueMessage<T>
                {
                    Payload = message,
                    Tag = lockedFilePath
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

                    var deadFilePath = Path.ChangeExtension(lockedFilePath, $".{DeadMessageExtension}");
                    File.Move(lockedFilePath, deadFilePath);
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

        // 큐가 비어있으면 null 반환
        return null;
    }

    /// <inheritdoc />
    public Task AckAsync(object tag, CancellationToken cancellationToken = default)
    {
        // tag는 lock 상태 파일의 경로입니다.
        if (tag is not string lockedFilePath)
            throw new InvalidOperationException("Invalid ack tag.");

        // lock 상태 파일이 존재하지 않으면 예외를 발생시킵니다.
        if (!File.Exists(lockedFilePath))
            throw new FileNotFoundException($"Lock file '{lockedFilePath}' does not exist.");

        // 확인된 메시지의 lock 상태 파일을 삭제합니다.
        File.Delete(lockedFilePath);
        
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task NackAsync(object tag, bool requeue = false, CancellationToken cancellationToken = default)
    {
        // tag는 lock 상태 파일의 경로입니다.
        if (tag is not string lockedFilePath)
            throw new InvalidOperationException("Invalid ack tag.");

        // lock 상태 파일이 존재하지 않으면 예외를 발생시킵니다.
        if (!File.Exists(lockedFilePath))
            throw new FileNotFoundException($"Lock file '{lockedFilePath}' does not exist.");

        // requeue가 true인 경우, lock 상태 파일을 원래 큐 파일로 이동합니다.
        if (requeue)
        {
            try
            {
                var queueFilePath = Path.ChangeExtension(lockedFilePath, $".{MessageExtension}");
                File.Move(lockedFilePath, queueFilePath);
            }
            catch (IOException)
            {
                // 이동 실패 시 lock 상태에 남겨둡니다.
            }
        }
        // 이외, lock 상태 파일을 삭제합니다.
        else
        {
            File.Delete(lockedFilePath);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 큐에 존재하는 처리되지 않은 메시지들을 다시 복원합니다.
    /// </summary>
    public async Task RestoreAsync()
    {
        var targets = new List<string>();
        targets.AddRange(Directory.GetFiles(_directoryPath, $"*.{LockExtension}"));
        targets.AddRange(Directory.GetFiles(_directoryPath, $"*.{DeadMessageExtension}"));

        await Parallel.ForEachAsync(targets, async (filePath, _) =>
        {
            try
            {
                // 유효하지 않거나 만료된 파일은 삭제합니다.
                if (IsInvalidOrExpired(filePath))
                {
                    File.Delete(filePath);
                }
                // 대상 파일을 큐 파일로 이동합니다.
                else
                {
                    var queueFilePath = Path.ChangeExtension(filePath, $".{MessageExtension}");
                    File.Move(filePath, queueFilePath, overwrite: false);
                }
            }
            catch (IOException)
            {
                // 충돌/접근 문제 발생 시 무시
            }

            await Task.CompletedTask;
        });
    }

    /// <summary>
    /// 파일 경로를 기반으로 파일명의 유효성과 메시지 만료 여부를 검사합니다.
    /// </summary>
    /// <returns>파일이 유효하지 않거나 만료된 경우 true, 유효한 경우 false</returns>
    private static bool IsInvalidOrExpired(string filePath)
    {
        // 파일명: "{enqueueTicks}_{expirationTicks}.ext"
        var parts = Path.GetFileNameWithoutExtension(filePath).Split('_');

        // 파일명 형식이 잘못됨
        if (parts.Length != 2)
        {
            return true;
        }

        // TTL이 설정되었고, 현재 시간이 만료 시간을 지났다면 유효하지 않음
        if (long.TryParse(parts[1], out long expirationTicks))
        {
            if (expirationTicks != 0 && DateTime.UtcNow.Ticks > expirationTicks)
            {
                return true;
            }
        }
        // 만료 정보가 숫자가 아닌 경우 유효하지 않음
        else
        {
            return true;
        }

        // 유효한 파일
        return false;
    }
}
