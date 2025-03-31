using IronHive.Abstractions.Memory;
using MessagePack;
using MessagePack.Resolvers;
using System.Text.Json;

namespace IronHive.Core.Storages;

public class LocalQueueStorage : IQueueStorage
{
    private const string DefaultName = ".hivemind";
    private readonly string _directory;
    private readonly MessagePackSerializerOptions _options = ContractlessStandardResolver.Options
        .WithCompression(MessagePackCompression.Lz4Block);

    public LocalQueueStorage(string? directoryPath = null)
    {
        _directory = directoryPath
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), DefaultName);
        Directory.CreateDirectory(_directory);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task EnqueueAsync<T>(T item, CancellationToken cancellationToken = default)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        var bytes = MessagePackSerializer.Serialize<T>(item, _options, cancellationToken);

        // 파일 이름: "UTC Ticks_GUID.json" 형식으로 생성하여 정렬
        var fileName = $"{DateTime.UtcNow.Ticks}_{Guid.NewGuid()}.msg";
        var filePath = Path.Combine(_directory, fileName);
        // 파일에 비동기로 쓰기
        await File.WriteAllBytesAsync(filePath, bytes, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T?> DequeueAsync<T>(CancellationToken cancellationToken = default)
    {
        // 큐 디렉토리 내의 JSON 파일 목록을 가져오고 이름 순으로 정렬 (파일명 앞부분이 UTC Ticks)
        var files = GetAllFiles().OrderBy(f => f).ToList();

        if (files.Count == 0)
            return default;

        var firstFile = files.First();

        try
        {
            // 파일 내용을 읽고 역직렬화
            var content = await File.ReadAllBytesAsync(firstFile, cancellationToken);
            var item = MessagePackSerializer.Deserialize<T>(content, _options, cancellationToken);

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
    public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        var count = GetAllFiles().Count();
        return Task.FromResult(count);
    }

    /// <inheritdoc />
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        foreach (var file in GetAllFiles())
        {
            try { File.Delete(file); } catch { /* 실패 시 무시 */ }
        }
        return Task.CompletedTask;
    }

    // 디렉토리에 해당하는 모든 파일이름을 가져옵니다.
    private IEnumerable<string> GetAllFiles()
    {
        return Directory.GetFiles(_directory, "*.msg");
    }
}