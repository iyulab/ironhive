using Amazon.S3.Model;
using Amazon.S3;
using IronHive.Abstractions.Memory;
using System.Net;

namespace IronHive.Storages.AmazonS3;

public class AmazonS3FileStorage : IFileStorage
{
    private readonly AmazonS3Config _config;
    private readonly AmazonS3Client _client;

    public AmazonS3FileStorage(AmazonS3Config config)
    {
        _config = config;
        // AmazonS3Client 생성 (config에 AccessKey, SecretKey, Region, BucketName 등이 있다고 가정)
        _client = new AmazonS3Client(new Amazon.S3.AmazonS3Config
        {
            
        });
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> ListAsync(
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ListObjectsV2Request
        {
            BucketName = _config.BucketName,
            Prefix = prefix,
        };
        var response = await _client.ListObjectsV2Async(request, cancellationToken);
        return response.S3Objects.Select(x => x.Key);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        string path, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 객체의 메타데이터를 가져와서 존재 여부 확인
            await _client.GetObjectMetadataAsync(_config.BucketName, path, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound)
                return false;
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Stream> ReadAsync(
        string filePath, 
        CancellationToken cancellationToken = default)
    {
        var response = await _client.GetObjectAsync(_config.BucketName, filePath, cancellationToken);
        var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;
        return memoryStream;
    }

    /// <inheritdoc />
    public async Task WriteAsync(
        string filePath, 
        Stream data, 
        bool overwrite = true, 
        CancellationToken cancellationToken = default)
    {
        if (!overwrite && await ExistsAsync(filePath, cancellationToken))
            throw new IOException($"파일이 이미 존재합니다: {filePath}");

        var request = new PutObjectRequest
        {
            BucketName = _config.BucketName,
            Key = filePath,
            InputStream = data
        };

        await _client.PutObjectAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        string path, 
        CancellationToken cancellationToken = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _config.BucketName,
            Key = path
        };
        await _client.DeleteObjectAsync(request, cancellationToken);
    }
}