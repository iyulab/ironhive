using IronHive.Abstractions.Memory;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronHive.Core.Storages;

/// <summary>
/// 로컬 파일시스템을 이용한 큐 스토리지 구현
/// </summary>
public class LocalQueueStorage : IQueueStorage
{
    private readonly string _queueDirectory;
    private readonly string _pendingDirectory;
    private readonly TimeSpan? _timeToLive;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// directoryPath가 null이면 유저 홈 디렉토리 아래 ".hivemind/queue"를 사용합니다.
    /// timeToLive가 null이면 무제한입니다.
    /// </summary>
    public LocalQueueStorage(string? directoryPath = null, TimeSpan? timeToLive = null)
    {
        _queueDirectory = directoryPath ?? LocalStorageConfig.DefaultQueueStoragePath;
        Directory.CreateDirectory(_queueDirectory);

        // pending 전용 폴더 생성 (동시 접근 및 Ack/Nack 처리를 위해)
        _pendingDirectory = Path.Combine(_queueDirectory, "pending");
        Directory.CreateDirectory(_pendingDirectory);

        _timeToLive = timeToLive;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task EnqueueAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        // 현재 UTC 시간을 기준으로 enqueueTicks를 생성합니다.
        var enqueueTicks = DateTime.UtcNow.Ticks;
        // TTL이 설정된 경우, 만료 시간을 계산합니다. 설정 되지 않은 경우 0으로 설정합니다.
        var expirationTicks = _timeToLive.HasValue ? DateTime.UtcNow.Add(_timeToLive.Value).Ticks : 0;

        var fileName = $"{enqueueTicks}_{expirationTicks}.msg";
        var filePath = Path.Combine(_queueDirectory, fileName);

        // 메시지 직렬화 후 파일에 저장
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(message, _jsonOptions);
        await File.WriteAllBytesAsync(filePath, bytes, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TaggedMessage<T>?> DequeueAsync<T>(CancellationToken cancellationToken = default)
    {
        // 큐 폴더 내의 .msg 파일들을 정렬하여 가져옵니다.
        var files = Directory.GetFiles(_queueDirectory, "*.msg")
            .OrderBy(f => f)
            .ToList();

        foreach (var file in files)
        {
            // pending 폴더로 원자적 이동 시도
            var pendingFilePath = Path.Combine(_pendingDirectory, Path.GetFileName(file));
            try
            {
                File.Move(file, pendingFilePath);
            }
            catch (IOException)
            {
                // 다른 소비자가 이미 처리 중일 경우 건너뜁니다.
                continue;
            }

            // 파일명에서 enqueueTicks와 만료 정보를 파싱합니다.
            // 파일명 형식: "{enqueueTicks}_{expirationInfo}.msg"
            var fileName = Path.GetFileNameWithoutExtension(pendingFilePath);
            var parts = fileName.Split('_');
            if (parts.Length != 2)
            {
                // 파일명 형식이 잘못된 경우 삭제합니다.
                File.Delete(pendingFilePath);
                continue;
            }

            var expirationInfo = parts[1];
            if (long.TryParse(expirationInfo, out long expirationTicks))
            {
                if (expirationTicks != 0 && (DateTime.UtcNow.Ticks > expirationTicks))
                {
                    // TTL이 만료된 파일인 경우 삭제합니다.
                    File.Delete(pendingFilePath);
                    continue;
                }
            }
            else
            {
                // 만료 정보가 잘못된 파일인 경우(숫자X) 삭제합니다.
                File.Delete(pendingFilePath);
                continue;
            }

            // 큐 안에 다른 타입의 메시지가 있을 경우 일단 Throw
            // IQueueStorage<T>로 구현 변경, 생각 해보기
            var bytes = await File.ReadAllBytesAsync(pendingFilePath, cancellationToken);
            var message = JsonSerializer.Deserialize<T>(bytes, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize message.");

            return new TaggedMessage<T>
            {
                Message = message,
                AckTag = pendingFilePath
            };
        }

        return null;
    }

    /// <inheritdoc />
    public async Task AckAsync(object ackTag, CancellationToken cancellationToken = default)
    {
        // ackTag는 pending 상태 파일의 경로입니다.
        if (ackTag is not string pendingFilePath)
            throw new InvalidOperationException("Invalid ack tag.");

        if (File.Exists(pendingFilePath))
        {
            // 확인된 메시지의 pending 상태 파일을 삭제합니다.
            File.Delete(pendingFilePath);
        }
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task NackAsync(object ackTag, bool requeue = false, CancellationToken cancellationToken = default)
    {
        if (ackTag is not string pendingFilePath)
            throw new InvalidOperationException("Invalid ack tag.");

        // pending 상태 파일이 존재하지 않으면 아무 작업도 하지 않습니다.
        if (!File.Exists(pendingFilePath))
        {
            await Task.CompletedTask;
            return;
        }

        if (requeue)
        {
            // requeue가 true인 경우, pending 상태 파일을 원래 큐 폴더로 이동합니다.
            var fileName = Path.GetFileName(pendingFilePath);
            var destinationPath = Path.Combine(_queueDirectory, fileName);
            try
            {
                File.Move(pendingFilePath, destinationPath);
            }
            catch (IOException)
            {
                // 이동 실패 시 pending 상태에 남겨둡니다.
            }
        }
        else
        {
            // requeue가 false인 경우, pending 상태 파일을 삭제합니다.
            File.Delete(pendingFilePath);
        }
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        var count = Directory.GetFiles(_queueDirectory, "*.msg").Length;
        return Task.FromResult(count);
    }

    /// <inheritdoc />
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        if (Directory.Exists(_queueDirectory))
        {
            Directory.Delete(_queueDirectory, recursive: true);
        }
        Directory.CreateDirectory(_queueDirectory);
        Directory.CreateDirectory(_pendingDirectory);
        return Task.CompletedTask;
    }
}
