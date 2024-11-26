using System.Collections.Concurrent;
using Raggle.Abstractions.Memory;

namespace Raggle.Driver.LocalDisk;

public class LocalDiskDocumentStorage : IDocumentStorage
{
    private readonly string _rootPath;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _collectionLocks = new();

    public LocalDiskDocumentStorage(LocalDiskConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.DirectoryPath))
            throw new ArgumentException("DirectoryPath는 비어 있을 수 없습니다.", nameof(config.DirectoryPath));

        _rootPath = config.DirectoryPath;
        Directory.CreateDirectory(_rootPath);
    }

    public void Dispose()
    {
        foreach (var semaphore in _collectionLocks.Values)
        {
            semaphore.Dispose();
        }
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
        var collectionPath = GetFullPath(collectionName);
        var isExist = Directory.Exists(collectionPath);
        return Task.FromResult(isExist);
    }

    /// <inheritdoc />
    public async Task CreateCollectionAsync(
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        var semaphore = GetLockForCollection(collectionName);
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var collectionPath = GetFullPath(collectionName);
            if (!Directory.Exists(collectionPath))
            {
                Directory.CreateDirectory(collectionPath);
            }
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task DeleteCollectionAsync(
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        var semaphore = GetLockForCollection(collectionName);
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var collectionPath = GetFullPath(collectionName);
            if (Directory.Exists(collectionPath))
            {
                Directory.Delete(collectionPath, true);
            }
        }
        finally
        {            
            semaphore.Release();
            _collectionLocks.TryRemove(collectionName, out var removedSemaphore);
            removedSemaphore?.Dispose();
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<string>> GetDocumentFilesAsync(
        string collectionName, 
        string documentId, 
        CancellationToken cancellationToken = default)
    {
        var documentPath = GetFullPath(collectionName, documentId);
        if (!Directory.Exists(documentPath))
            throw new InvalidOperationException($"'{documentId}' 문서는 존재하지 않습니다.");

        var files = Directory.EnumerateFiles(documentPath, "*", SearchOption.AllDirectories)
                             .Select(path => Path.GetRelativePath(documentPath, path));
        return Task.FromResult(files);
    }

    /// <inheritdoc />
    public Task<bool> DocumentFileExistsAsync(
        string collectionName, 
        string documentId, 
        string filePath, 
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(collectionName, documentId, filePath);
        var isExist = File.Exists(fullPath);
        return Task.FromResult(isExist);
    }

    /// <inheritdoc />
    public async Task WriteDocumentFileAsync(
        string collectionName, 
        string documentId, 
        string filePath, 
        Stream content, 
        bool overwrite = true, 
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(collectionName, documentId, filePath);
        if (!overwrite && File.Exists(fullPath))
            throw new IOException($"파일이 이미 존재합니다: {fullPath}");

        var directory = Path.GetDirectoryName(fullPath);
        if (string.IsNullOrEmpty(directory))
            throw new ArgumentException("잘못된 파일 경로입니다.", nameof(filePath));
        Directory.CreateDirectory(directory);

        using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
        {
            await content.CopyToAsync(fileStream, 81920, cancellationToken); // 버퍼 사이즈 조정 가능
        }
    }

    /// <inheritdoc />
    public async Task<Stream> ReadDocumentFileAsync(
        string collectionName, 
        string documentId, 
        string filePath, 
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(collectionName, documentId, filePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"파일을 찾을 수 없습니다: {filePath}", fullPath);

        var memoryStream = new MemoryStream();
        using (var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, useAsync: true))
        {
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
        var fullPath = GetFullPath(collectionName, documentId, filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        await Task.CompletedTask;
    }

    #region Private Methods

    // 컬렉션별 SemaphoreSlim을 반환 (쓰기 전용)
    private SemaphoreSlim GetLockForCollection(string collectionName)
    {
        return _collectionLocks.GetOrAdd(collectionName, _ => new SemaphoreSlim(1, 1));
    }

    // 실제 디스크의 전체 경로를 반환
    private string GetFullPath(params string[] paths)
    {
        var allPaths = new List<string> { _rootPath };
        allPaths.AddRange(paths);
        return Path.Combine(allPaths.ToArray());
    }

    #endregion
}
