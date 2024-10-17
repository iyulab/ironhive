using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using MessagePack;
using Raggle.Abstractions.Memory;
using System.Reflection.Metadata.Ecma335;

namespace Raggle.Document.Azure;

public class AzureBlobDocumentStorage : IDocumentStorage
{
    private const string DocumentIndexFileName = "document_index.msgpack";
    private readonly BlobServiceClient _client;

    public AzureBlobDocumentStorage(AzureBlobConfig config, BlobClientOptions? options = null)
    {
        _client = GetBlobStorageClient(config, options);
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
        var index = await GetDocumentIndexAsync(collection);
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
        return query;
    }

    /// <inheritdoc />
    public async Task<DocumentDetail> GetDocumentDetailAsync(string collection, string documentId, CancellationToken cancellationToken = default)
    {
        var filter = new MemoryFilterBuilder().AddDocumentId(documentId).Build();
        var records = await FindDocumentRecordsAsync(collection, filter, cancellationToken).ConfigureAwait(false);
        var record = records.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Document ID '{documentId}' not found in collection '{collection}'");

        return new DocumentDetail
        {
            EmbeddingStatus = record.EmbeddingStatus,
            DocumentId = record.DocumentId,
            FileName = record.FileName,
            ContentType = record.ContentType,
            Size = record.Size,
            Tags = record.Tags,
            CreatedAt = record.CreatedAt,
            LastUpdatedAt = record.LastUpdatedAt,
        };
    }

    /// <inheritdoc />
    public async Task UpsertDocumentRecordAsync(string collection, DocumentRecord document, CancellationToken cancellationToken = default)
    {
        var index = await GetDocumentIndexAsync(collection);

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task DeleteDocumentRecordAsync(string collection, string documentId, CancellationToken cancellationToken = default)
    {
        var index = await GetDocumentIndexAsync(collection);

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task UploadFileAsync(string collection, string documentId, string filePath, Stream Content, bool overwrite = true, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(collection);
        var blobName = CombinePath(documentId, filePath);
        var blob = container.GetBlobClient(blobName);
        await blob.UploadAsync(Content, overwrite, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Stream> DownloadFileAsync(string collection, string documentId, string filePath, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(collection);
        var blobName = CombinePath(documentId, filePath);
        var blob = container.GetBlobClient(blobName);
        var stream = await blob.OpenReadAsync(cancellationToken: cancellationToken);
        return stream;
    }

    /// <inheritdoc />
    public async Task DeleteFileAsync(string collection, string documentId, string filePath, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(collection);
        var blobName = CombinePath(documentId, filePath);
        var blob = container.GetBlobClient(blobName);
        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    #region Private Methods

    // 경로 조합
    private string CombinePath(string path1, string path2)
    {
        return $"{path1.TrimEnd('/')}/{path2.TrimStart('/')}";
    }

    // Document Index 파일 데이터를 가져오기
    private async Task<IEnumerable<DocumentRecord>> GetDocumentIndexAsync(string collection)
    {
        var container = _client.GetBlobContainerClient(collection);
        var blob = container.GetBlobClient(DocumentIndexFileName);
        if (!blob.Exists()) return [];

        var content = await blob.DownloadContentAsync();
        var data = content.Value.Content;
        return MessagePackSerializer.Deserialize<IEnumerable<DocumentRecord>>(data);
    }

    // BlobServiceClient 생성
    private BlobServiceClient GetBlobStorageClient(AzureBlobConfig config, BlobClientOptions? options)
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
        ThrowIfNullOrWhiteSpace(config.AccountName, nameof(config.AccountName));
        return new Uri($"https://{config.AccountName}.blob.core.windows.net");
    }

    // AccountKey를 이용한 인증 방식
    private StorageSharedKeyCredential GetSharedKeyCredential(AzureBlobConfig config)
    {
        ThrowIfNullOrWhiteSpace(config.AccountKey, nameof(config.AccountKey));
        return new StorageSharedKeyCredential(config.AccountName, config.AccountKey);
    }

    // SAS Token을 이용한 인증 방식
    private AzureSasCredential GetSasTokenCredential(AzureBlobConfig config)
    {
        ThrowIfNullOrWhiteSpace(config.SASToken, nameof(config.SASToken));
        return new AzureSasCredential(config.SASToken);
    }

    // Null 또는 공백 문자열 검사
    private void ThrowIfNullOrWhiteSpace(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"The {paramName} cannot be null or whitespace.", paramName);
        }
    }

    #endregion
}
