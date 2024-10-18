using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MessagePack;
using Raggle.Abstractions.Memory;

namespace Raggle.Document.Azure;

public class AzureBlobDocumentStorage : IDocumentStorage
{
    private const string DocumentIndexFileName = "document_index.msgpack";
    private const int MaxRetryAttempts = 3;
    private const int DelayBetweenRetriesMs = 200;

    private readonly BlobServiceClient _client;

    public AzureBlobDocumentStorage(AzureBlobConfig config, BlobClientOptions? options = null)
    {
        _client = GetBlobServiceClient(config, options);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetAllCollectionsAsync(CancellationToken cancellationToken = default)
    {
        var collections = new List<string>();
        await foreach (var container in _client.GetBlobContainersAsync(cancellationToken: cancellationToken))
        {
            if (container.IsDeleted == true) continue;
            collections.Add(container.Name);
        }
        return collections;
    }

    /// <inheritdoc />
    public async Task CreateCollectionAsync(string collection, CancellationToken cancellationToken = default)
    {
        await _client.CreateBlobContainerAsync(collection, cancellationToken: cancellationToken); 
    }

    /// <inheritdoc />
    public async Task DeleteCollectionAsync(string collection, CancellationToken cancellationToken = default)
    {
        await _client.DeleteBlobContainerAsync(collection, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentRecord>> FindDocumentRecordsAsync(string collection, MemoryFilter? filter = null, CancellationToken cancellationToken = default)
    {
        var (index, _) = await GetDocumentIndexWithETagAsync(collection, cancellationToken);
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
        return query ?? Enumerable.Empty<DocumentRecord>();
    }

    /// <inheritdoc />
    public async Task UpsertDocumentRecordAsync(string collection, DocumentRecord document, CancellationToken cancellationToken = default)
    {
        int retryCount = 0;
        while (retryCount < MaxRetryAttempts)
        {
            var (index, etag) = await GetDocumentIndexWithETagAsync(collection, cancellationToken);

            // 기존 문서 제거 후 새 문서 추가
            index.RemoveAll(x => x.DocumentId == document.DocumentId);
            index.Add(document);

            var data = MessagePackSerializer.Serialize(index);
            var container = GetBlobContainerClient(collection);
            var blob = container.GetBlobClient(DocumentIndexFileName);

            try
            {
                using var stream = new MemoryStream(data);
                var uploadOptions = new BlobUploadOptions
                {
                    Conditions = new BlobRequestConditions
                    {
                        IfMatch = etag
                    }
                };
                await blob.UploadAsync(stream, uploadOptions, cancellationToken);
                return; // Success
            }
            catch (RequestFailedException ex) when (ex.Status == 412)
            {
                // ETag 불일치, 다른 프로세스가 blob을 수정함. 재시도.
                retryCount++;
                await Task.Delay(DelayBetweenRetriesMs, cancellationToken);
            }
        }

        throw new InvalidOperationException("Failed to upsert document record due to concurrent modifications.");
    }

    /// <inheritdoc />
    public async Task DeleteDocumentRecordAsync(string collection, string documentId, CancellationToken cancellationToken = default)
    {
        int retryCount = 0;
        while (retryCount < MaxRetryAttempts)
        {
            var (index, etag) = await GetDocumentIndexWithETagAsync(collection, cancellationToken);
            int removedCount = index.RemoveAll(x => x.DocumentId == documentId);

            if (removedCount == 0)
            {
                // 문서를 찾을 수 없음; 삭제할 필요 없음
                return;
            }

            var data = MessagePackSerializer.Serialize(index);
            var container = GetBlobContainerClient(collection);
            var blob = container.GetBlobClient(DocumentIndexFileName);

            try
            {
                using var stream = new MemoryStream(data);
                var uploadOptions = new BlobUploadOptions
                {
                    Conditions = new BlobRequestConditions
                    {
                        IfMatch = etag,
                    }
                };
                await blob.UploadAsync(stream, uploadOptions, cancellationToken);
                return; // Success
            }
            catch (RequestFailedException ex) when (ex.Status == 412)
            {
                // ETag 불일치, 다른 프로세스가 blob을 수정함. 재시도.
                retryCount++;
                await Task.Delay(DelayBetweenRetriesMs, cancellationToken);
            }
        }

        throw new InvalidOperationException("Failed to delete document record due to concurrent modifications.");
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetDocumentFilesAsync(string collection, string documentId, CancellationToken cancellationToken = default)
    {
        var container = GetBlobContainerClient(collection);
        var prefix = $"{documentId}/";
        var files = new List<string>();
        await foreach (var blob in container.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken))
        {
            files.Add(blob.Name);
        }
        return files;
    }

    /// <inheritdoc />
    public async Task WriteDocumentFileAsync(string collection, string documentId, string filePath, Stream content, bool overwrite = true, CancellationToken cancellationToken = default)
    {
        var blob = GetBlobClient(collection, documentId, filePath);
        await blob.UploadAsync(content, overwrite, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Stream> ReadDocumentFileAsync(string collection, string documentId, string filePath, CancellationToken cancellationToken = default)
    {
        var blob = GetBlobClient(collection, documentId, filePath);
        var stream = await blob.OpenReadAsync(cancellationToken: cancellationToken);
        return stream;
    }

    /// <inheritdoc />
    public async Task DeleteDocumentFileAsync(string collection, string documentId, string filePath, CancellationToken cancellationToken = default)
    {
        var blob = GetBlobClient(collection, documentId, filePath);
        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    #region Private Methods

    // 경로 조합: 두 경로 사이에 '/'가 하나만 있도록 함
    private static string CombinePath(string path1, string path2)
    {
        return $"{path1.TrimEnd('/')}/{path2.TrimStart('/')}";
    }

    // ETag와 함께 문서 인덱스를 가져옴
    private async Task<(List<DocumentRecord>, ETag?)> GetDocumentIndexWithETagAsync(string collection, CancellationToken cancellationToken)
    {
        var container = GetBlobContainerClient(collection);
        var blob = container.GetBlobClient(DocumentIndexFileName);
        if (!await blob.ExistsAsync(cancellationToken))
        {
            return ([], null);
        }

        var downloadInfo = await blob.DownloadAsync(cancellationToken);
        var data = downloadInfo.Value.Content;
        var index = MessagePackSerializer.Deserialize<List<DocumentRecord>>(data);
        var etag = downloadInfo.Value.Details.ETag;
        return (index, etag);
    }

    // BlobClient 생성
    private BlobClient GetBlobClient(string collection, string documentId, string filePath)
    {
        var container = GetBlobContainerClient(collection);
        var blobName = CombinePath(documentId, filePath);
        return container.GetBlobClient(blobName);
    }

    // BlobContainerClient 생성
    private BlobContainerClient GetBlobContainerClient(string collection)
    {
        var container = _client.GetBlobContainerClient(collection);
        container.CreateIfNotExists();
        return container;
    }

    // BlobServiceClient 생성
    private BlobServiceClient GetBlobServiceClient(AzureBlobConfig config, BlobClientOptions? options)
    {
        var client = config.AuthType switch
        {
            AzureBlobAuthTypes.ConnectionString => new BlobServiceClient(config.ConnectionString, options),
            AzureBlobAuthTypes.AccountKey => new BlobServiceClient(GetBlobStorageUri(config), GetSharedKeyCredential(config), options),
            AzureBlobAuthTypes.SASToken => new BlobServiceClient(GetBlobStorageUri(config), GetSasTokenCredential(config), options),
            AzureBlobAuthTypes.AzureIdentity => new BlobServiceClient(GetBlobStorageUri(config), config.TokenCredential, options),
            _ => throw new ArgumentOutOfRangeException(nameof(config.AuthType), "Unknown Azure Blob authentication type")
        };
        return client;
    }

    // AccountName를 이용한 Uri 생성
    private Uri GetBlobStorageUri(AzureBlobConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.AccountName))
            throw new ArgumentException("The AccountName cannot be null or whitespace.", nameof(config.AccountName));
        return new Uri($"https://{config.AccountName}.blob.core.windows.net");
    }

    // AccountKey를 이용한 인증 방식
    private StorageSharedKeyCredential GetSharedKeyCredential(AzureBlobConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.AccountName))
            throw new ArgumentException("The AccountName cannot be null or whitespace.", nameof(config.AccountName));
        return new StorageSharedKeyCredential(config.AccountName, config.AccountKey);
    }

    // SAS Token을 이용한 인증 방식
    private AzureSasCredential GetSasTokenCredential(AzureBlobConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.SASToken))
            throw new ArgumentException("The SASToken cannot be null or whitespace.", nameof(config.SASToken));
        return new AzureSasCredential(config.SASToken);
    }

    #endregion
}
