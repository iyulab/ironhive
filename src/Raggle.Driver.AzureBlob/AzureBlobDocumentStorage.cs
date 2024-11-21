using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MessagePack;
using Raggle.Abstractions.Memory;
using System.Text.RegularExpressions;

namespace Raggle.Driver.AzureBlob;

public class AzureBlobDocumentStorage : IDocumentStorage
{
    private const string DocumentIndexFileName = "document_index.msgpack";
    private static readonly Regex _collectionNameRegex = new(@"^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);
    private readonly Random _random = new();

    private readonly BlobServiceClient _client;
    private readonly int _maxRetryAttempts;
    private readonly int _baseDelayMilliseconds;

    public AzureBlobDocumentStorage(AzureBlobConfig config, BlobClientOptions? options = null)
    {
        _client = GetBlobServiceClient(config, options);
        _maxRetryAttempts = config.BlobMaxRetryAttempts;
        _baseDelayMilliseconds = config.BlobDelayMilliseconds;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetCollectionListAsync(CancellationToken cancellationToken = default)
    {
        var collections = new List<string>();
        await foreach (BlobContainerItem container in _client.GetBlobContainersAsync(cancellationToken: cancellationToken))
        {
            collections.Add(container.Name);
        }
        return collections;
    }

    /// <inheritdoc />
    public async Task<bool> CollectionExistsAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(collectionName);
        return await container.ExistsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CreateCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        if (!_collectionNameRegex.IsMatch(collectionName))
            throw new ArgumentException("유효하지 않은 컬렉션 이름입니다. 소문자, 숫자, 하이픈(-)만 사용할 수 있습니다.", nameof(collectionName));

        var container = _client.GetBlobContainerClient(collectionName);
        await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        // 인덱스 파일 초기화
        var indexBlob = container.GetBlobClient(DocumentIndexFileName);
        var emptyIndex = new List<DocumentRecord>();
        var serializedIndex = MessagePackSerializer.Serialize(emptyIndex);
        using (var stream = new MemoryStream(serializedIndex))
        {
            await indexBlob.UploadAsync(stream, overwrite: true, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(collectionName);
        await container.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentRecord>> FindDocumentsAsync(string collectionName, MemoryFilter? filter = null, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(collectionName);
        var indexBlob = container.GetBlobClient(DocumentIndexFileName);

        var (index, _) = await LoadDocumentIndexWithETagAsync(indexBlob, cancellationToken);
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
        var container = _client.GetBlobContainerClient(collectionName);
        var indexBlob = container.GetBlobClient(DocumentIndexFileName);
        
        var (index, _) = await LoadDocumentIndexWithETagAsync(indexBlob, cancellationToken);
        return index.Any(x => x.DocumentId == documentId);
    }

    /// <inheritdoc />
    public async Task<DocumentRecord> UpsertDocumentAsync(DocumentRecord document, Stream? content = null, CancellationToken cancellationToken = default)
    {
        int retryAttempts = 0;
        while (retryAttempts < _maxRetryAttempts)
        {
            try
            {
                var container = _client.GetBlobContainerClient(document.CollectionName);
                var indexBlob = container.GetBlobClient(DocumentIndexFileName);

                // 인덱스 로드
                var (index, currentETag) = await LoadDocumentIndexWithETagAsync(indexBlob, cancellationToken);

                // 문서 찾기
                var profile = index.FirstOrDefault(x => x.DocumentId == document.DocumentId);
                if (profile == null && content == null)
                    throw new InvalidOperationException($"'{document.DocumentId}' 문서를 찾을 수 없습니다.");

                if (content != null)
                {
                    await WriteDocumentFileAsync(document.CollectionName, document.DocumentId, document.FileName, content, true, cancellationToken);
                }

                // 문서 추가 또는 업데이트
                index.RemoveAll(x => x.DocumentId == document.DocumentId);
                document.LastUpdatedAt = DateTime.UtcNow;
                index.Add(document);
                await SaveDocumentIndexAsync(indexBlob, index, currentETag, cancellationToken);

                return document;
            }
            catch (RequestFailedException ex) when (ex.Status == 412) // Precondition Failed
            {
                retryAttempts++;
                int delay = CalculateBackoffDelay(retryAttempts);
                await Task.Delay(delay, cancellationToken);
            }
        }
        throw new InvalidOperationException("최대 재시도 횟수를 초과했습니다. 작업을 완료할 수 없습니다.");
    }

    /// <inheritdoc />
    public async Task DeleteDocumentAsync(string collectionName, string documentId, CancellationToken cancellationToken = default)
    {
        int retryAttempts = 0;
        while (retryAttempts < _maxRetryAttempts)
        {
            try
            {
                var container = _client.GetBlobContainerClient(collectionName);
                var indexBlob = container.GetBlobClient(DocumentIndexFileName);

                // 인덱스 로드
                var (index, currentETag) = await LoadDocumentIndexWithETagAsync(indexBlob, cancellationToken);

                // 문서 존재 여부 확인
                var document = index.FirstOrDefault(x => x.DocumentId == documentId);
                if (document == null)
                    throw new InvalidOperationException($"'{documentId}' 문서는 존재하지 않습니다.");

                // 문서 파일 삭제
                string prefix = $"{documentId}/";
                await foreach (BlobItem blob in container.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken))
                {
                    var blobClient = container.GetBlobClient(blob.Name);
                    await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
                }

                // 인덱스 업데이트
                index.RemoveAll(x => x.DocumentId == documentId);
                await SaveDocumentIndexAsync(indexBlob, index, currentETag, cancellationToken);

                return;
            }
            catch (RequestFailedException ex) when (ex.Status == 412) // Precondition Failed
            {
                retryAttempts++;
                int delay = CalculateBackoffDelay(retryAttempts);
                await Task.Delay(delay, cancellationToken);
            }
        }
        throw new InvalidOperationException("최대 재시도 횟수를 초과했습니다. 작업을 완료할 수 없습니다.");
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetDocumentFilesAsync(string collectionName, string documentId, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(collectionName);
        string prefix = $"{documentId}/";
        var files = new List<string>();

        await foreach (BlobItem blob in container.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken))
        {
            var relativePath = blob.Name.Substring(prefix.Length);
            files.Add(relativePath);
        }

        return files;
    }

    /// <inheritdoc />
    public async Task<bool> DocumentFileExistsAsync(string collectionName, string documentId, string filePath, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(collectionName);
        var blobName = Path.Combine(documentId, filePath).Replace("\\", "/");
        var blobClient = container.GetBlobClient(blobName);
        return await blobClient.ExistsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task WriteDocumentFileAsync(string collectionName, string documentId, string filePath, Stream content, bool overwrite = true, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(collectionName);
        var blobName = Path.Combine(documentId, filePath).Replace("\\", "/");
        var blobClient = container.GetBlobClient(blobName);

        if (!overwrite && await blobClient.ExistsAsync(cancellationToken))
            throw new IOException($"파일이 이미 존재합니다: {blobName}");

        // 파일 업로드 또는 덮어쓰기
        await blobClient.UploadAsync(content, overwrite, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Stream> ReadDocumentFileAsync(string collectionName, string documentId, string filePath, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(collectionName);
        var blobName = Path.Combine(documentId, filePath).Replace("\\", "/");
        var blobClient = container.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync(cancellationToken))
            throw new FileNotFoundException($"파일을 찾을 수 없습니다: {filePath}");

        var memoryStream = new MemoryStream();
        await blobClient.DownloadToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;
        return memoryStream;
    }

    /// <inheritdoc />
    public async Task DeleteDocumentFileAsync(string collectionName, string documentId, string filePath, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(collectionName);
        var blobName = Path.Combine(documentId, filePath).Replace("\\", "/");
        var blobClient = container.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    #region Private Methods

    // BlobServiceClient 생성
    private BlobServiceClient GetBlobServiceClient(AzureBlobConfig config, BlobClientOptions? options)
    {
        var client = config.AuthType switch
        {
            AzureBlobAuthTypes.ConnectionString => new BlobServiceClient(config.ConnectionString, options),
            AzureBlobAuthTypes.AccountKey => new BlobServiceClient(GetBlobStorageUri(config), GetSharedKeyCredential(config), options),
            AzureBlobAuthTypes.SASToken => new BlobServiceClient(GetBlobStorageUri(config), GetSasTokenCredential(config), options),
            AzureBlobAuthTypes.AzureIdentity => new BlobServiceClient(GetBlobStorageUri(config), config.TokenCredential, options),
            _ => throw new ArgumentOutOfRangeException(nameof(config.AuthType), "알 수 없는 Azure Blob 인증 유형입니다.")
        };
        return client;
    }

    // AccountName을 이용한 Uri 생성
    private static Uri GetBlobStorageUri(AzureBlobConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.AccountName))
            throw new ArgumentException("AccountName은 비어 있을 수 없습니다.", nameof(config.AccountName));
        return new Uri($"https://{config.AccountName}.blob.core.windows.net");
    }

    // AccountKey를 이용한 인증 방식
    private static StorageSharedKeyCredential GetSharedKeyCredential(AzureBlobConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.AccountName))
            throw new ArgumentException("AccountName은 비어 있을 수 없습니다.", nameof(config.AccountName));
        return new StorageSharedKeyCredential(config.AccountName, config.AccountKey);
    }

    // SAS Token을 이용한 인증 방식
    private static AzureSasCredential GetSasTokenCredential(AzureBlobConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.SASToken))
            throw new ArgumentException("SASToken은 비어 있을 수 없습니다.", nameof(config.SASToken));
        return new AzureSasCredential(config.SASToken);
    }

    // 인덱스 파일 로드 with ETag
    private async Task<(List<DocumentRecord>, ETag?)> LoadDocumentIndexWithETagAsync(BlobClient indexBlob, CancellationToken cancellationToken)
    {
        if (!await indexBlob.ExistsAsync(cancellationToken))
            throw new InvalidOperationException($"'{indexBlob.Name}' 인덱스 파일이 존재하지 않습니다.");

        using var stream = new MemoryStream();
        await indexBlob.DownloadToAsync(stream, cancellationToken);
        stream.Position = 0;
        var index = await MessagePackSerializer.DeserializeAsync<List<DocumentRecord>>(stream, cancellationToken: cancellationToken);
        var eTag = indexBlob.GetProperties().Value.ETag;
        return (index, eTag);
    }

    // 인덱스 파일 저장 with ETag
    private async Task SaveDocumentIndexAsync(BlobClient indexBlob, List<DocumentRecord> index, ETag? eTag, CancellationToken cancellationToken)
    {
        var serializedIndex = MessagePackSerializer.Serialize(index);
        using var stream = new MemoryStream(serializedIndex);
        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = "application/x-msgpack" },
            Conditions = new BlobRequestConditions { IfMatch = eTag }
        };
        await indexBlob.UploadAsync(stream, uploadOptions, cancellationToken);
    }

    // 재시도 지연 계산 지수 방식
    private int CalculateBackoffDelay(int retryAttempts)
    {
        int maxDelay = 60_000; // 최대 지연 시간 1분
        int delay = _baseDelayMilliseconds * (int)Math.Pow(2, retryAttempts - 1);
        return Math.Min(delay, maxDelay) + _random.Next(0, 100); // 지터 추가
    }

    #endregion
}
