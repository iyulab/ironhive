using System.Collections.Concurrent;
using MessagePack;
using Raggle.Abstractions.Memory;

namespace Raggle.DocumentStorage.LocalDisk;

public class LocalDiskDocumentStorage : IDocumentStorage
{
    private const string DocumentIndexFileName = "document_index.msgpack";
    private readonly Random _random = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _collectionLocks = new();

    private readonly string _rootPath;
    private readonly int _maxRetryAttempts;
    private readonly int _baseDelayMilliseconds;

    public LocalDiskDocumentStorage(LocalDiskStorageConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.DirectoryPath))
            throw new ArgumentException("DirectoryPath는 비어 있을 수 없습니다.", nameof(config.DirectoryPath));

        _rootPath = config.DirectoryPath;
        Directory.CreateDirectory(_rootPath);
        _maxRetryAttempts = config.BlobMaxRetryAttempts > 0 ? config.BlobMaxRetryAttempts : 5; // 기본값 5
        _baseDelayMilliseconds = config.BlobDelayMilliseconds > 0 ? config.BlobDelayMilliseconds : 200; // 기본값 200ms
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
    public Task<IEnumerable<string>> GetCollectionListAsync(CancellationToken cancellationToken = default)
    {
        var directories = Directory.EnumerateDirectories(_rootPath);
        var collections = directories.Select(path => Path.GetFileName(path)) ?? Enumerable.Empty<string>();
        return Task.FromResult(collections);
    }

    /// <inheritdoc />
    public Task<bool> CollectionExistsAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        var collectionPath = GetFullPath(collectionName);
        var isExist = Directory.Exists(collectionPath);
        return Task.FromResult(isExist);
    }

    /// <inheritdoc />
    public async Task CreateCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        var semaphore = GetLockForCollection(collectionName);
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var collectionPath = GetFullPath(collectionName);
            if (!Directory.Exists(collectionPath))
            {
                Directory.CreateDirectory(collectionPath);
                var indexPath = GetFullPath(collectionName, DocumentIndexFileName);
                var emptyIndex = new List<DocumentRecord>();
                var serializedIndex = MessagePackSerializer.Serialize(emptyIndex);
                await File.WriteAllBytesAsync(indexPath, serializedIndex, cancellationToken);
            }
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
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
    public async Task<IEnumerable<DocumentRecord>> FindDocumentsAsync(string collectionName, MemoryFilter? filter = null, CancellationToken cancellationToken = default)
    {
        // 읽기 작업은 잠금 없이 진행
        var (index, _) = await LoadDocumentIndexWithTimeStampAsync(collectionName, cancellationToken);
        if (filter == null) return index;

        var query = index.AsQueryable();
        if (filter.DocumentIds != null && filter.DocumentIds.Count != 0)
        {
            query = query.Where(x => filter.DocumentIds.Contains(x.DocumentId));
        }
        if (filter.Tags != null && filter.Tags.Count != 0)
        {
            query = query.Where(x => x.Tags.Intersect(filter.Tags).Any());
        }
        return query.ToList();
    }

    /// <inheritdoc />
    public async Task<bool> ExistDocumentAsync(string collectionName, string documentId, CancellationToken cancellationToken = default)
    {
        // 읽기 작업은 잠금 없이 진행
        var (index, _) = await LoadDocumentIndexWithTimeStampAsync(collectionName, cancellationToken);
        return index.Any(x => x.DocumentId == documentId);
    }

    /// <inheritdoc />
    public async Task<DocumentRecord> UpsertDocumentAsync(DocumentRecord document, Stream? content = null, CancellationToken cancellationToken = default)
    {
        var collectionName = document.CollectionName;
        var semaphore = GetLockForCollection(collectionName);
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var isExist = await ExistDocumentAsync(collectionName, document.DocumentId, cancellationToken);
            if (!isExist && content == null)
                throw new InvalidOperationException($"'{document.DocumentId}' 문서를 찾을 수 없습니다.");

            if (content != null)
            {
                await WriteDocumentFileAsync(collectionName, document.DocumentId, document.FileName, content, overwrite: true, cancellationToken: cancellationToken);
            }

            int retryAttempt = 0;
            while (retryAttempt < _maxRetryAttempts)
            {
                try
                {
                    var (index, timeStamp) = await LoadDocumentIndexWithTimeStampAsync(collectionName, cancellationToken);
                    index.RemoveAll(x => x.DocumentId == document.DocumentId);
                    document.LastUpdatedAt = DateTime.UtcNow;
                    index.Add(document);
                    await SaveDocumentIndexAsync(collectionName, index, timeStamp, cancellationToken);
                    return document; // 성공 시 반환
                }
                catch (IOException)
                {
                    retryAttempt++;
                    var delay = CalculateBackoffDelay(retryAttempt);
                    await Task.Delay(delay, cancellationToken);
                }
            }
            throw new InvalidOperationException($"문서 인덱스를 업데이트하는 데 실패했습니다. 최대 재시도 횟수 {_maxRetryAttempts}를 초과했습니다.");
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task DeleteDocumentAsync(string collectionName, string documentId, CancellationToken cancellationToken = default)
    {
        var semaphore = GetLockForCollection(collectionName);
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var documentPath = GetFullPath(collectionName, documentId);
            if (Directory.Exists(documentPath))
            {
                Directory.Delete(documentPath, true);
            }

            int retryAttempt = 0;
            while (retryAttempt < _maxRetryAttempts)
            {
                try
                {
                    var (index, timeStamp) = await LoadDocumentIndexWithTimeStampAsync(collectionName, cancellationToken);
                    index.RemoveAll(x => x.DocumentId == documentId);
                    await SaveDocumentIndexAsync(collectionName, index, timeStamp, cancellationToken);
                    return; // 성공 시 반환
                }
                catch (IOException)
                {
                    retryAttempt++;
                    var delay = CalculateBackoffDelay(retryAttempt);
                    await Task.Delay(delay, cancellationToken);
                }
            }
            throw new InvalidOperationException($"문서 인덱스를 업데이트하는 데 실패했습니다. 최대 재시도 횟수 {_maxRetryAttempts}를 초과했습니다.");
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<string>> GetDocumentFilesAsync(string collectionName, string documentId, CancellationToken cancellationToken = default)
    {
        var documentPath = GetFullPath(collectionName, documentId);
        if (!Directory.Exists(documentPath))
            throw new InvalidOperationException($"'{documentId}' 문서는 존재하지 않습니다.");

        var files = Directory.EnumerateFiles(documentPath, "*", SearchOption.AllDirectories)
                             .Select(path => Path.GetRelativePath(documentPath, path));
        return Task.FromResult(files);
    }

    /// <inheritdoc />
    public Task<bool> DocumentFileExistsAsync(string collectionName, string documentId, string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(collectionName, documentId, filePath);
        var isExist = File.Exists(fullPath);
        return Task.FromResult(isExist);
    }

    /// <inheritdoc />
    public async Task WriteDocumentFileAsync(string collectionName, string documentId, string filePath, Stream content, bool overwrite = true, CancellationToken cancellationToken = default)
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
    public async Task<Stream> ReadDocumentFileAsync(string collectionName, string documentId, string filePath, CancellationToken cancellationToken = default)
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
    public async Task DeleteDocumentFileAsync(string collectionName, string documentId, string filePath, CancellationToken cancellationToken = default)
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

    // 인덱스 파일을 로드
    private async Task<(List<DocumentRecord>, DateTime)> LoadDocumentIndexWithTimeStampAsync(string collectionName, CancellationToken cancellationToken)
    {
        var indexPath = GetFullPath(collectionName, DocumentIndexFileName);
        if (!File.Exists(indexPath))
            throw new InvalidOperationException($"'{collectionName}' 컬렉션에 인덱스 파일이 존재하지 않습니다.");

        var lastModified = File.GetLastWriteTimeUtc(indexPath);
        using (var stream = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, useAsync: true))
        {
            var index = await MessagePackSerializer.DeserializeAsync<List<DocumentRecord>>(stream, cancellationToken: cancellationToken);
            return (index, lastModified);
        }
    }

    // 인덱스 파일을 저장
    private async Task SaveDocumentIndexAsync(string collectionName, IEnumerable<DocumentRecord> index, DateTime timeStamp, CancellationToken cancellationToken)
    {
        var indexPath = GetFullPath(collectionName, DocumentIndexFileName);
        if (!File.Exists(indexPath))
            throw new InvalidOperationException($"'{collectionName}' 컬렉션에 인덱스 파일이 존재하지 않습니다.");

        if (File.GetLastWriteTimeUtc(indexPath) != timeStamp)
            throw new IOException("인덱스 파일이 다른 프로세스에 의해 변경되었습니다.");

        var tempPath = indexPath + ".tmp";
        using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
        {
            await MessagePackSerializer.SerializeAsync(stream, index, cancellationToken: cancellationToken);
        }

        File.Replace(tempPath, indexPath, null);
    }

    // 재시도 지연 계산 지수 방식 + 지터
    private int CalculateBackoffDelay(int retryAttempts)
    {
        int maxDelay = 60_000; // 최대 지연 시간 1분
        int delay = _baseDelayMilliseconds * (int)Math.Pow(2, retryAttempts - 1);
        return Math.Min(delay, maxDelay) + _random.Next(0, 100); // 지터 추가
    }

    #endregion
}
