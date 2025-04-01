using IronHive.Abstractions.Memory;
using System.Text.Json;

namespace IronHive.Core.Storages;

public class LocalQueueStorage : IQueueStorage
{
    private readonly string _directoryPath;

    public LocalQueueStorage(string? directoryPath = null)
    {
        _directoryPath ??= LocalStorageConfig.DefaultQueueStoragePath;
        Directory.CreateDirectory(_directoryPath);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public Task<IEnumerable<string>> ListQueuesAsync(
        CancellationToken cancellationToken = default)
    {
        // _baseDirectoryPath 하위의 모든 디렉터리 이름을 큐 이름으로 간주
        var queues = Directory.GetDirectories(_directoryPath)
                              .Select(p => p.Substring(_directoryPath.Length + 1))
                              .AsEnumerable()
                              ?? Enumerable.Empty<string>();
        return Task.FromResult(queues);
    }

    /// <inheritdoc />
    public Task<bool> ExistsQueueAsync(
        string name, 
        CancellationToken cancellationToken = default)
    {
        var queuePath = GetQueuePath(name);
        var exists = Directory.Exists(queuePath);
        return Task.FromResult(exists);
    }

    /// <inheritdoc />
    public Task CreateQueueAsync(
        string name, 
        CancellationToken cancellationToken = default)
    {
        var queuePath = GetQueuePath(name);
        Directory.CreateDirectory(queuePath);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteQueueAsync(
        string name, 
        CancellationToken cancellationToken = default)
    {
        var queuePath = GetQueuePath(name);
        if (Directory.Exists(queuePath))
        {
            Directory.Delete(queuePath, recursive: true);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task EnqueueAsync<T>(
        string name,
        T item, 
        CancellationToken cancellationToken = default)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        var queuePath = GetQueuePath(name);
        Directory.CreateDirectory(queuePath);

        // 파일 이름: "UTC Ticks_GUID.msg" 형식으로 생성하여 정렬
        var bytes = item.Serialize() ?? [];
        var fileName = $"{DateTime.UtcNow.Ticks}_{Guid.NewGuid()}.msg";
        var filePath = Path.Combine(queuePath, fileName);
        // 파일에 비동기로 쓰기
        await File.WriteAllBytesAsync(filePath, bytes, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T?> DequeueAsync<T>(
        string name,
        CancellationToken cancellationToken = default)
    {
        // 큐 디렉토리 내의 MSG 파일 목록을 가져오고 이름 순으로 정렬 (파일명 앞부분이 UTC Ticks)
        var files = GetQueueFiles(name).OrderBy(f => f).ToList();

        if (files.Count == 0)
            return default;

        var firstFile = files.First();

        try
        {
            // 파일 내용을 읽고 역직렬화
            var content = await File.ReadAllBytesAsync(firstFile, cancellationToken);
            var item = content.Deserialize<T>();

            // !! 메시지 소비 후 파일 삭제
            File.Delete(firstFile);
            return item;
        }
        catch
        {
            return default;
        }
    }

    /// <inheritdoc />
    public Task<int> CountAsync(
        string name, 
        CancellationToken cancellationToken = default)
    {
        var count = GetQueueFiles(name).Length;
        return Task.FromResult(count);
    }

    /// <inheritdoc />
    public Task ClearAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var queuePath = GetQueuePath(name);
        Directory.Delete(queuePath, recursive: true);
        Directory.CreateDirectory(_directoryPath);
        return Task.CompletedTask;
    }

    // 주어진 큐 이름에 대한 전체 경로 반환
    private string GetQueuePath(string name)
    {
        return Path.Combine(_directoryPath, name);
    }

    // 디렉토리에 해당하는 모든 파일이름을 가져옵니다.
    private string[] GetQueueFiles(string name)
    {
        var queuePath = GetQueuePath(name);
        return Directory.GetFiles(queuePath, "*.msg");
    }
}
