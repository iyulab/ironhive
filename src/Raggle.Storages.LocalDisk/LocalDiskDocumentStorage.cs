using System.Runtime.CompilerServices;
using Raggle.Abstractions.Memory;

namespace Raggle.Storages.LocalDisk;

public class LocalDiskDocumentStorage : IDocumentStorage
{
    private readonly string _rootPath;
    private const int _defaultBufferSize = 81920; // 기본 80KB
    private const int _maxBufferSize = 1048576;   // 최대 1MB
    private const int _minBufferSize = 4096;      // 최소 4KB

    public LocalDiskDocumentStorage(LocalDiskConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.DirectoryPath))
            throw new ArgumentException("DirectoryPath는 비어 있을 수 없습니다.", nameof(config.DirectoryPath));

        _rootPath = config.DirectoryPath;
        Directory.CreateDirectory(_rootPath);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public Task<IEnumerable<string>> GetCollectionListAsync(
        CancellationToken cancellationToken = default)
    {
        var directories = Directory.EnumerateDirectories(_rootPath);
        var collections = directories.Select(path => Path.GetFileName(path)) ?? Enumerable.Empty<string>();
        return Task.FromResult(collections);
    }

    /// <inheritdoc />
    public Task<bool> CollectionExistsAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        var collectionPath = GetAbsolutePath(collectionName);
        var isExist = Directory.Exists(collectionPath);
        return Task.FromResult(isExist);
    }

    /// <inheritdoc />
    public async Task CreateCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        var collectionPath = GetAbsolutePath(collectionName);
        if (!Directory.Exists(collectionPath))
        {
            Directory.CreateDirectory(collectionPath);
        }
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task DeleteCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        var collectionPath = GetAbsolutePath(collectionName);
        if (Directory.Exists(collectionPath))
        {
            Directory.Delete(collectionPath, true);
        }
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> GetDocumentFilesAsync(
        string collectionName,
        string documentId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var documentPath = GetAbsolutePath(collectionName, documentId);
        if (!Directory.Exists(documentPath))
            throw new InvalidOperationException($"'{documentId}' 문서는 존재하지 않습니다.");

        foreach (var path in Directory.EnumerateFiles(documentPath, "*", SearchOption.AllDirectories))
        {
            // 취소 요청이 있는지 확인
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = Path.GetRelativePath(documentPath, path);
            yield return relativePath;
            await Task.Yield();
        }
    }

    /// <inheritdoc />
    public Task<bool> DocumentFileExistsAsync(
        string collectionName,
        string documentId,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetAbsolutePath(collectionName, documentId, filePath);
        var isExist = File.Exists(fullPath);
        return Task.FromResult(isExist);
    }

    /// <inheritdoc />
    public async Task WriteDocumentFileAsync(
        string collectionName,
        string documentId,
        string filePath,
        Stream data,
        bool overwrite = true,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetAbsolutePath(collectionName, documentId, filePath);
        if (!overwrite && File.Exists(fullPath))
            throw new IOException($"파일이 이미 존재합니다: {fullPath}");

        var directory = Path.GetDirectoryName(fullPath);
        if (string.IsNullOrEmpty(directory))
            throw new ArgumentException("잘못된 파일 경로입니다.", nameof(filePath));
        Directory.CreateDirectory(directory);

        using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
        {
            var bufferSize = GetOptimalBufferSize(data.Length);
            await data.CopyToAsync(fileStream, bufferSize, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<Stream> ReadDocumentFileAsync(
        string collectionName,
        string documentId,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetAbsolutePath(collectionName, documentId, filePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"파일을 찾을 수 없습니다: {filePath}", fullPath);

        var memoryStream = new MemoryStream();
        using (var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, useAsync: true))
        {
            var fileSize = new FileInfo(fullPath).Length;
            var bufferSize = GetOptimalBufferSize(fileSize);
            await fileStream.CopyToAsync(memoryStream, 81920, cancellationToken);
        }
        memoryStream.Position = 0;
        return memoryStream;
    }

    /// <inheritdoc />
    public async Task DeleteDocumentFileAsync(
        string collectionName,
        string documentId,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetAbsolutePath(collectionName, documentId, filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        var dirPath = GetAbsolutePath(collectionName, documentId);
        if (!Directory.EnumerateFileSystemEntries(dirPath).Any())
        {
            Directory.Delete(dirPath);
        }
        await Task.CompletedTask;
    }

    #region Private Methods

    // 실제 디스크의 전체 경로를 반환
    private string GetAbsolutePath(params string[] paths)
    {
        var allPaths = new List<string> { _rootPath };
        allPaths.AddRange(paths);
        return Path.Combine(allPaths.ToArray());
    }

    // 스트림 크기에 따라 최적의 버퍼 크기를 반환합니다.
    private int GetOptimalBufferSize(long length)
    {
        if (length <= 0) return _defaultBufferSize;

        // 스트림 크기에 따라 동적으로 버퍼 크기 조정
        if (length < 1 * 1024 * 1024) // 1MB 이하
            return _minBufferSize;
        if (length < 100 * 1024 * 1024) // 100MB 이하
            return _defaultBufferSize;

        // 100MB 이상 대용량 파일
        return _maxBufferSize;
    }

    #endregion
}
