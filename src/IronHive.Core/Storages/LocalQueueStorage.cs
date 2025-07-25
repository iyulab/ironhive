using IronHive.Abstractions.Memory;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronHive.Core.Storages;

/// <summary>
/// 로컬 파일시스템을 이용한 큐 스토리지 구현
/// </summary>
public class LocalQueueStorage : IQueueStorage
{
    private string _directoryPath = string.Empty;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// 큐 저장소의 디렉토리 경로입니다.
    /// </summary>
    public required string DirectoryPath
    {
        get => _directoryPath;
        init
        {
            _directoryPath = value;
            Directory.CreateDirectory(DirectoryPath);
            RestoreQueueMessages();
        }
    }

    /// <summary>
    /// 큐 메시지가 살아 있는 시간(Time To Live, TTL)입니다.
    /// 지정하지 않는 경우 메시지는 만료되지 않습니다.
    /// </summary>
    public TimeSpan? TimeToLive { get; init; }

    /// <inheritdoc />
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
        var expirationTicks = TimeToLive.HasValue ? DateTime.UtcNow.Add(TimeToLive.Value).Ticks : 0;

        var fileName = $"{enqueueTicks}_{expirationTicks}.msg";
        var filePath = Path.Combine(DirectoryPath, fileName);

        // 메시지 직렬화 후 파일에 저장
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(message, _jsonOptions);
        await File.WriteAllBytesAsync(filePath, bytes, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TaggedMessage<T>?> DequeueAsync<T>(CancellationToken cancellationToken = default)
    {
        // 큐 폴더 내의 .msg 파일들을 정렬하여 가져옵니다.
        var files = Directory.GetFiles(DirectoryPath, "*.msg").OrderBy(f => f).ToList();

        foreach (var file in files)
        {
            // 메시지 파일을 잠금상태로 변경합니다.
            var lockedFilePath = Path.ChangeExtension(file, ".lock");
            try
            {
                File.Move(file, lockedFilePath);
            }
            catch (IOException)
            {
                // 다른 소비자가 이미 처리 중일 경우 건너뜁니다.
                continue;
            }

            // 파일명에서 enqueueTicks와 만료 정보를 파싱합니다.
            // 파일명 형식: "{enqueueTicks}_{expirationInfo}.lock"
            var fileName = Path.GetFileNameWithoutExtension(lockedFilePath);
            var parts = fileName.Split('_');
            if (parts.Length != 2)
            {
                // 파일명 형식이 잘못된 경우 삭제합니다.
                File.Delete(lockedFilePath);
                continue;
            }

            var expirationInfo = parts[1];
            if (long.TryParse(expirationInfo, out long expirationTicks))
            {
                if (expirationTicks != 0 && (DateTime.UtcNow.Ticks > expirationTicks))
                {
                    // TTL이 만료된 파일인 경우 삭제합니다.
                    File.Delete(lockedFilePath);
                    continue;
                }
            }
            else
            {
                // 만료 정보가 잘못된 파일인 경우(숫자X) 삭제합니다.
                File.Delete(lockedFilePath);
                continue;
            }

            // 큐 안에 다른 타입의 메시지가 있을 경우 일단 Throw
            // IQueueStorage<T>로 구현 변경, 생각 해보기
            var bytes = await File.ReadAllBytesAsync(lockedFilePath, cancellationToken);
            var message = JsonSerializer.Deserialize<T>(bytes, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize message.");

            return new TaggedMessage<T>
            {
                Message = message,
                AckTag = lockedFilePath
            };
        }

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
            var originalPath = Path.ChangeExtension(lockedFilePath, ".msg");
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
        var count = Directory.GetFiles(DirectoryPath, "*.msg").Length;
        return Task.FromResult(count);
    }

    /// <inheritdoc />
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        if (Directory.Exists(DirectoryPath))
        {
            Directory.Delete(DirectoryPath, recursive: true);
        }
        Directory.CreateDirectory(DirectoryPath);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 시스템 시작 시 처리 중(lock 상태)의 메시지를 다시 큐에 복구합니다.
    /// </summary>
    private void RestoreQueueMessages()
    {
        var lockedFiles = Directory.GetFiles(DirectoryPath, "*.lock");

        foreach (var lockedFile in lockedFiles)
        {
            var msgFile = Path.ChangeExtension(lockedFile, ".msg");

            try
            {
                File.Move(lockedFile, msgFile, overwrite: false); // 이미 있을 경우 덮어쓰지 않음
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
