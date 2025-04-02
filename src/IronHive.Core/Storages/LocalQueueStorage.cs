using IronHive.Abstractions.Memory;
using System.Text;
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
    public async Task EnqueueAsync(string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentNullException(nameof(message));

        var bytes = Encoding.UTF8.GetBytes(message);
        var fileName = $"{DateTime.UtcNow.Ticks}_{Guid.NewGuid().ToString()}.msg";
        var filePath = Path.Combine(_directoryPath, fileName);
        // 파일에 비동기로 쓰기
        await File.WriteAllBytesAsync(filePath, bytes, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        // 큐 디렉토리 내의 MSG 파일 목록을 가져오고 이름 순으로 정렬 (파일명 앞부분이 UTC Ticks)
        var files = GetMessageFiles().OrderBy(f => f).ToList();

        if (files.Count == 0)
            return default;

        var firstFile = files.First();

        try
        {
            // 파일 내용을 읽고 디코딩
            var bytes = await File.ReadAllBytesAsync(firstFile, cancellationToken);
            var item = Encoding.UTF8.GetString(bytes);

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
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        var count = GetMessageFiles().Length;
        return Task.FromResult(count);
    }

    /// <inheritdoc />
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        Directory.Delete(_directoryPath, recursive: true);
        Directory.CreateDirectory(_directoryPath);
        return Task.CompletedTask;
    }

    // 디렉토리에 해당하는 모든 파일이름을 가져옵니다.
    private string[] GetMessageFiles()
    {
        return Directory.GetFiles(_directoryPath, "*.msg");
    }
}
