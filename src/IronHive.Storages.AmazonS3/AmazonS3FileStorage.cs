using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using IronHive.Abstractions.Files;
using System.Net;
using AmazonS3ClientConfig = Amazon.S3.AmazonS3Config;

namespace IronHive.Storages.AmazonS3;

public class AmazonS3FileStorage : IFileStorage
{
    private readonly AmazonS3Client _client;

    public string BucketName { get; init; }

    public AmazonS3FileStorage(AmazonS3Config config)
    {
        if (string.IsNullOrWhiteSpace(config.BucketName))
            throw new ArgumentException("S3 버킷 이름이 필요합니다.", nameof(config.BucketName));

        BucketName = config.BucketName;
        _client = CreateClient(config);
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
        // S3에서는 디렉터리 개념이 가상의 prefix로 표현됩니다.
        // prefix가 null 또는 공백이면 빈 문자열로 처리합니다.
        prefix ??= string.Empty;

        // Prefix ~ Delimiter 사이의 객체만 가져옵니다.
        var request = new ListObjectsV2Request
        {
            BucketName = BucketName,
            Prefix = prefix,
            Delimiter = "/",
        };

        var result = new List<string>();
        ListObjectsV2Response response;

        do
        {
            response = await _client.ListObjectsV2Async(request, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            // 파일 목록 추가
            if (response.S3Objects != null)
            {
                result.AddRange(response.S3Objects.Select(obj => obj.Key));
            }

            // "디렉터리" 목록 추가 (CommonPrefixes에는 접미사 "/"가 포함되어 있습니다.)
            if (response.CommonPrefixes != null)
            {
                result.AddRange(response.CommonPrefixes);
            }

            // 다음 페이지가 있는 경우, ContinuationToken을 설정하여 다음 페이지를 요청합니다.
            request.ContinuationToken = response.NextContinuationToken;
        }
        while (response.IsTruncated); // 페이지 처리가 끝날 때까지 반복

        return result;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        if (IsDirectory(path))
        {
            // 디렉터리인 경우, 해당 prefix로 시작하는 객체가 있는지 확인합니다.
            var response = await _client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = BucketName,
                Prefix = path,
                MaxKeys = 1
            }, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return response.KeyCount > 0;
        }
        else
        {
            try
            {
                // 개별 객체 메타데이터를 가져와 존재 여부 확인
                var metadata = await _client.GetObjectMetadataAsync(BucketName, path, cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                return metadata != null;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }
    }

    /// <inheritdoc />
    public async Task<Stream> ReadAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (IsDirectory(filePath))
            throw new ArgumentException("디렉터리 경로로 파일을 읽을 수 없습니다.", nameof(filePath));
        if (!await ExistsAsync(filePath, cancellationToken).ConfigureAwait(false))
            throw new FileNotFoundException($"파일을 찾을 수 없습니다: {filePath}");

        // S3에서 객체를 가져와 메모리 스트림에 복사합니다.
        using var response = await _client.GetObjectAsync(BucketName, filePath, cancellationToken).ConfigureAwait(false);
        using var responseStream = response.ResponseStream;
        var memoryStream = new MemoryStream();
        await responseStream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
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
        if (IsDirectory(filePath))
            throw new ArgumentException("디렉터리 경로로 파일을 쓸 수 없습니다.", nameof(filePath));
        if (!overwrite && await ExistsAsync(filePath, cancellationToken).ConfigureAwait(false))
            throw new IOException($"파일이 이미 존재합니다: {filePath}");

        // S3 PutObjectRequest에 스트림 전달
        var response = await _client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = BucketName,
            Key = filePath,
            InputStream = data,
        }, cancellationToken).ConfigureAwait(false);
        data.Position = 0;

        if (response.HttpStatusCode != HttpStatusCode.OK)
            throw new IOException($"파일 업로드에 실패했습니다: {filePath}");
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        if (IsDirectory(path))
        {
            // 디렉터리인 경우 해당 prefix로 시작하는 모든 객체를 삭제합니다.
            var listRequest = new ListObjectsV2Request
            {
                BucketName = BucketName,
                Prefix = path
            };

            ListObjectsV2Response listResponse;
            var objects = new List<KeyVersion>();

            do
            {
                listResponse = await _client.ListObjectsV2Async(listRequest, cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();

                objects.AddRange(listResponse.S3Objects.Select(obj => new KeyVersion { Key = obj.Key }));
                listRequest.ContinuationToken = listResponse.NextContinuationToken;
            }
            while (listResponse.IsTruncated);

            if (objects.Count != 0)
            {
                // S3에서는 한 번에 최대 1000개 삭제 가능
                foreach (var chunks in objects.Chunk(1000))
                {
                    var res = await _client.DeleteObjectsAsync(new DeleteObjectsRequest
                    {
                        BucketName = BucketName,
                        Objects = chunks.ToList()
                    }, cancellationToken).ConfigureAwait(false);
                }
            }
        }
        else
        {
            // 개별 객체 삭제
            await _client.DeleteObjectAsync(BucketName, path, cancellationToken).ConfigureAwait(false);
        }
    }

    // S3에서는 키가 "/"로 끝나면 가상의 디렉터리로 간주합니다.
    private static bool IsDirectory(string path)
    {
        return path.EndsWith('/');
    }

    // AWS 클라이언트 생성
    private AmazonS3Client CreateClient(AmazonS3Config config)
    {
        if (string.IsNullOrWhiteSpace(config.AccessKey))
            throw new ArgumentException("AWS IAM 액세스 키가 필요합니다.");

        if (string.IsNullOrWhiteSpace(config.SecretAccessKey))
            throw new ArgumentException("AWS IAM 비밀 액세스 키가 필요합니다.");

        if (string.IsNullOrWhiteSpace(config.RegionCode))
            throw new ArgumentException("AWS S3 서비스가 위치한 지역 코드가 필요합니다.");

        var clientConfig = new AmazonS3ClientConfig
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(config.RegionCode),
        };

        return new AmazonS3Client(
            awsAccessKeyId: config.AccessKey,
            awsSecretAccessKey: config.SecretAccessKey,
            clientConfig: clientConfig);
    }
}
