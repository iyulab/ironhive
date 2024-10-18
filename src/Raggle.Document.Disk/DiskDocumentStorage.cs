using Raggle.Abstractions.Memory;
using System.Collections.Concurrent;
using MessagePack;

namespace Raggle.Document.Disk;

public class DiskDocumentStorage : IDocumentStorage
{
    private const string DocumentIndexFileName = "document_index.msgpack";
    private const int MaxRetryAttempts = 3;
    private const int DelayBetweenRetriesMs = 200;

    private readonly string _rootPath;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _collectionLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

    public DiskDocumentStorage(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
            throw new ArgumentNullException(nameof(rootPath));

        _rootPath = rootPath;
        Directory.CreateDirectory(rootPath);
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
    public Task<IEnumerable<string>> GetAllCollectionsAsync(CancellationToken cancellationToken = default)
    {
        var directories = Directory.EnumerateDirectories(_rootPath);
        var collections = directories.Select(path => Path.GetFileName(path)) ?? [];
        return Task.FromResult(collections);
    }

    /// <inheritdoc />
    public Task CreateCollectionAsync(string collection, CancellationToken cancellationToken = default)
    {
        GetCollectionPath(collection);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteCollectionAsync(string collection, CancellationToken cancellationToken = default)
    {
        var collectionPath = GetCollectionPath(collection);
        if (Directory.Exists(collectionPath))
        {
            Directory.Delete(collectionPath, true);
        }

        // Semaphore 제거
        _collectionLocks.TryRemove(collection, out var semaphore);
        semaphore?.Dispose();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentRecord>> FindDocumentRecordsAsync(string collection, MemoryFilter? filter = null, CancellationToken cancellationToken = default)
    {
        var index = await LoadIndexAsync(collection, cancellationToken);
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
    public async Task UpsertDocumentRecordAsync(string collection, DocumentRecord document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        int retryCount = 0;
        while (retryCount < MaxRetryAttempts)
        {
            var semaphore = GetSemaphore(collection);
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var index = await LoadIndexAsync(collection, cancellationToken);

                // 기존 문서 제거 후 새 문서 추가
                index.RemoveAll(x => x.DocumentId == document.DocumentId);
                index.Add(document);

                await SaveIndexAsync(collection, index, cancellationToken);
                return; // 성공
            }
            catch (IOException)
            {
                // 파일 접근 중 문제 발생 시 재시도
                retryCount++;
                await Task.Delay(DelayBetweenRetriesMs, cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        }

        throw new InvalidOperationException("동시 수정으로 인해 문서 레코드를 업서트할 수 없습니다.");
    }

    /// <inheritdoc />
    public async Task DeleteDocumentRecordAsync(string collection, string documentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(documentId))
            throw new ArgumentException("DocumentId cannot be null or whitespace.", nameof(documentId));

        int retryCount = 0;
        while (retryCount < MaxRetryAttempts)
        {
            var semaphore = GetSemaphore(collection);
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var index = await LoadIndexAsync(collection, cancellationToken);

                // 문서 삭제
                int removedCount = index.RemoveAll(x => x.DocumentId == documentId);
                if (removedCount == 0)
                {
                    // 문서를 찾을 수 없음; 삭제할 필요 없음
                    return;
                }

                await SaveIndexAsync(collection, index, cancellationToken);
                return; // 성공
            }
            catch (IOException)
            {
                // 파일 접근 중 문제 발생 시 재시도
                retryCount++;
                await Task.Delay(DelayBetweenRetriesMs, cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        }

        throw new InvalidOperationException("동시 수정으로 인해 문서 레코드를 삭제할 수 없습니다.");
    }

    /// <inheritdoc />
    public Task<IEnumerable<string>> GetDocumentFilesAsync(string collection, string documentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(documentId))
            throw new ArgumentException("DocumentId cannot be null or whitespace.", nameof(documentId));

        var documentPath = Path.Combine(GetCollectionPath(collection), documentId);
        if (!Directory.Exists(documentPath))
            return Task.FromResult(Enumerable.Empty<string>());

        var files = Directory.EnumerateFiles(documentPath, "*", SearchOption.AllDirectories)
                             .Select(path => Path.GetRelativePath(documentPath, path));
        return Task.FromResult(files);
    }

    /// <inheritdoc />
    public async Task WriteDocumentFileAsync(string collection, string documentId, string filePath, Stream content, bool overwrite = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var fullPath = Path.Combine(GetCollectionPath(collection), documentId, filePath);

        if (!overwrite && File.Exists(fullPath))
            throw new IOException($"The file already exist: {fullPath}");

        var directory = Path.GetDirectoryName(fullPath);
        if (string.IsNullOrEmpty(directory))
            throw new ArgumentException("Invalid file path.", nameof(filePath));
        Directory.CreateDirectory(directory);
        
        using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<Stream> ReadDocumentFileAsync(string collection, string documentId, string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(GetCollectionPath(collection), documentId, filePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"파일을 찾을 수 없습니다: {fullPath}");

        var memoryStream = new MemoryStream();
        using (var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            await fileStream.CopyToAsync(memoryStream, cancellationToken);
        }
        memoryStream.Position = 0;
        return memoryStream;
    }

    /// <inheritdoc />
    public Task DeleteDocumentFileAsync(string collection, string documentId, string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(GetCollectionPath(collection), documentId, filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }

    #region Private Methods

    // 컬렉션의 전체 경로를 반환 (없으면 생성)
    private string GetCollectionPath(string collection)
    {
        if (string.IsNullOrEmpty(collection))
            throw new ArgumentException("Collection name cannot be null or whitespace.", nameof(collection));
        var collectionPath = Path.Combine(_rootPath, collection);
        Directory.CreateDirectory(collectionPath);
        return collectionPath;
    }

    // 인덱스 파일의 전체 경로를 반환
    private string GetIndexFilePath(string collection)
    {
        return Path.Combine(GetCollectionPath(collection), DocumentIndexFileName);
    }

    // SemaphoreSlim을 반환 (없으면 생성)
    private SemaphoreSlim GetSemaphore(string collection)
    {
        return _collectionLocks.GetOrAdd(collection, _ => new SemaphoreSlim(1, 1));
    }

    // 인덱스 파일을 로드
    private async Task<List<DocumentRecord>> LoadIndexAsync(string collection, CancellationToken cancellationToken)
    {
        var indexPath = GetIndexFilePath(collection);
        if (!File.Exists(indexPath))
        {
            return [];
        }

        using (var stream = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            return await MessagePackSerializer.DeserializeAsync<List<DocumentRecord>>(stream, cancellationToken: cancellationToken);
        }
    }

    // 인덱스 파일을 저장
    private async Task SaveIndexAsync(string collection, IEnumerable<DocumentRecord> index, CancellationToken cancellationToken)
    {
        var indexPath = GetIndexFilePath(collection);
        var tempPath = indexPath + ".tmp";

        using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await MessagePackSerializer.SerializeAsync(stream, index, cancellationToken: cancellationToken);
        }

        if (File.Exists(indexPath))
        {
            // 파일이 있으면 교체
            File.Replace(tempPath, indexPath, null);
        }
        else
        {
            // 파일이 없으면 이동
            File.Move(tempPath, indexPath);
        }
    }

    #endregion
}
