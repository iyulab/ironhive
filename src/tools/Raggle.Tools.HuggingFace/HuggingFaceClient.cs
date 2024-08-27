using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Web;

namespace Raggle.Tools.HuggingFace;

public class HuggingFaceClient
{
    private const string HF_HOST = "huggingface.co";
    private const string HF_MODELS_GET_PATH = "/api/models";
    private readonly string? _token;

    public HuggingFaceClient(string? token = null)
    {
        _token = token;
    }

    // Chat filter: text-generation
    // Embedding filter: sentence-similarity
    // llama.cpp filter: gguf
    public async Task<HuggingFaceModel[]> GetModelsAsync(
        string? search,
        string[]? filters = null,
        int? limit = 5,
        CancellationToken cancellationToken = new())
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["sort"] = "downloads";
        query["direction"] = "-1";
        query["full"] = "true";
        query["config"] = "false";
        if (!string.IsNullOrWhiteSpace(search))
        {
            query["search"] = search;
        }
        if (filters != null && filters.Length > 0)
        {
            query["filter"] = string.Join(",", filters.Select(filter => filter.Trim()));
        }
        if (limit != null && limit > 0)
        {
            query["limit"] = limit.ToString();
        }
        
        var requestUri = new UriBuilder
        {
            Scheme = "https",
            Host = HF_HOST,
            Path = HF_MODELS_GET_PATH,
            Query = query.ToString()
        }.ToString();

        var client = GetHttpClient();
        var response = await client.GetFromJsonAsync<HuggingFaceModel[]>(requestUri, cancellationToken);
        return response ?? [];
    }

    public async IAsyncEnumerable<FileDownloadProgress> DownloadFileAsync(
        string repoId,
        string filePath,
        string outputPath, // The path to save the uploaded file full path
        [EnumeratorCancellation] CancellationToken cancellationToken = new())
    {
        var requestUri = new UriBuilder
        {
            Scheme = "https",
            Host = HF_HOST,
            Path = $"/{repoId}/resolve/main/{filePath}",
            Query = "download=true"
        }.ToString();

        var client = GetHttpClient();
        using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength;
        var hasTotalBytes = totalBytes != null;

        EnsureDirectory(outputPath);
        using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[8192];
        long totalBytesRead = 0;
        int bytesRead;

        var progress = new FileDownloadProgress
        {
            UploadPath = outputPath,
            TotalBytes = totalBytes
        };

        var startTime = DateTime.UtcNow;
        while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) != 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            totalBytesRead += bytesRead;

            var elapsedTime = DateTime.UtcNow - startTime;
            var downloadSpeed = totalBytesRead / elapsedTime.TotalSeconds;

            progress.CurrentBytes = totalBytesRead;
            progress.DownloadSpeed = downloadSpeed;
            if (hasTotalBytes && downloadSpeed > 0)
            {
                progress.DownloadProgress = (double?)totalBytesRead / totalBytes;
                progress.RemainingTime = TimeSpan.FromSeconds((totalBytes!.Value - totalBytesRead) / downloadSpeed);
            }
            yield return progress;
        }

        progress.IsCompleted = true;
        progress.DownloadSpeed = 0;
        progress.DownloadProgress = 1;
        progress.RemainingTime = TimeSpan.Zero;
        yield return progress;
    }

    public async IAsyncEnumerable<RepoDownloadProgress> DownloadRepoAsync(
        string repoId,
        string outputDir,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var requestUri = new UriBuilder
        {
            Scheme = "https",
            Host = HF_HOST,
            Path = $"{HF_MODELS_GET_PATH}/{repoId}",
        }.ToString();

        var client = GetHttpClient();
        var response = await client.GetFromJsonAsync<HuggingFaceModel>(requestUri, cancellationToken);
        var files = response?.GetFilePaths() ?? Array.Empty<string>();

        // 디렉토리 생성
        foreach (var file in files)
        {
            var outputPath = Path.Combine(outputDir, file);
            var dirPath = Path.GetDirectoryName(outputPath)
                ?? throw new InvalidOperationException("유효하지 않은 디렉토리 경로입니다.");
            EnsureDirectory(dirPath);
        }

        var progress = new RepoDownloadProgress
        {
            TotalFiles = files,
        };

        var downloadProgresses = new ConcurrentDictionary<string, FileDownloadProgress>();
        var semaphore = new SemaphoreSlim(5); // 동시에 최대 5개의 다운로드 허용
        var tasks = files.Select(async file =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var outputPath = Path.Combine(outputDir, file);
                await foreach (var fileProgress in DownloadFileAsync(repoId, file, outputPath, cancellationToken))
                {
                    downloadProgresses[file] = fileProgress;
                }
                downloadProgresses.TryRemove(file, out _);
                progress.CompletedFiles.Add(file);
            }
            catch (Exception ex)
            {
                // 예외 처리 로직 추가 가능
                Console.Error.WriteLine($"파일 다운로드 중 오류 발생: {file}, 오류: {ex.Message}");
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        while (!tasks.All(t => t.IsCompleted))
        {
            progress.CurrentProgresses = downloadProgresses.Values.ToList();
            yield return progress;
            await Task.Delay(500, cancellationToken); // 0.5초마다 진행 상황 업데이트
        }

        await Task.WhenAll(tasks); // 모든 다운로드 작업 완료 대기

        progress.IsCompleted = true;
        yield return progress;
    }

    private HttpClient GetHttpClient()
    {
        var client = new HttpClient();

        if (!string.IsNullOrWhiteSpace(_token) && _token.StartsWith("hf_"))
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
        }

        return client;
    }

    private static void EnsureDirectory(string path)
    {
        var directory = Path.GetDirectoryName(path) 
            ?? throw new InvalidOperationException("The directory path is invalid.");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
