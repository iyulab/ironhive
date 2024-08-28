using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Web;
using Raggle.Tools.ModelSearch.Models;
using Raggle.Tools.ModelSearch.Progress;

namespace Raggle.Tools.ModelSearch.Clients;

/// <summary>
/// a search and download client for interacting with the Hugging Face API.
/// </summary>
public class HuggingFaceClient
{
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="HuggingFaceClient"/> class.
    /// </summary>
    /// <param name="token">
    /// The Hugging Face API token used for authentication. 
    /// If no token is provided, the client will operate in an unauthenticated mode, 
    /// which may limit access to certain models or repositories, particularly private or restricted ones.
    /// Without a token, downloading some models might not be possible.
    /// </param>
    public HuggingFaceClient(string? token = null)
    {
        _client = CreateHttpClient(token);
    }

    /// <summary>
    /// Asynchronously retrieves a list of models from the Hugging Face API based on the provided search query and filters.
    /// </summary>
    /// <param name="search">
    /// The search query string to filter models by name or description. If null or empty, all models will be considered.
    /// </param>
    /// <param name="filters">
    /// An optional array of filters to narrow down the search results. 
    /// Common filter examples include "text-generation", "gguf", and "sentence-similarity". 
    /// If null or empty, no additional filters will be applied.
    /// </param>
    /// <param name="limit">
    /// The maximum number of models to retrieve. If not specified, a default of 5 models will be returned.
    /// </param>
    /// <returns>
    /// The task result contains an array of <see cref="HuggingFaceModel"/> objects
    /// </returns>
    public async Task<IEnumerable<HuggingFaceModel>> GetModelsAsync(
        string? search = null,
        string[]? filters = null,
        int limit = 5,
        CancellationToken cancellationToken = default)
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
        if (limit > 0)
        {
            query["limit"] = limit.ToString();
        }

        var requestUri = new UriBuilder
        {
            Scheme = "https",
            Host = Constants.HF_HOST,
            Path = Constants.HF_GET_MODELS_PATH,
            Query = query.ToString()
        }.ToString();

        var models = await _client.GetFromJsonAsync<HuggingFaceModel[]>(requestUri, cancellationToken);
        return models ?? [];
    }

    /// <summary>
    /// Downloads a file from the Hugging Face API.
    /// </summary>
    /// <param name="repoId">The ID of the repository.</param>
    /// <param name="filePath">The path of the file in the repository.</param>
    /// <param name="outputPath">The path to save the downloaded file.</param>
    /// <returns>An asynchronous enumerable of <see cref="FileDownloadProgress"/> objects.</returns>
    public async IAsyncEnumerable<FileDownloadProgress> DownloadFileAsync(
        string repoId,
        string filePath,
        string outputPath,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!Path.HasExtension(outputPath))
        {
            outputPath = Path.Combine(outputPath, Path.GetFileName(filePath));
        }
        EnsureDirectory(outputPath);

        var requestUri = new UriBuilder
        {
            Scheme = "https",
            Host = Constants.HF_HOST,
            Path = $"/{repoId}/resolve/main/{filePath}",
            Query = "download=true"
        }.ToString();

        using var response = await _client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength;
        var hasTotalBytes = totalBytes != null;

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

    /// <summary>
    /// Downloads a repository from the Hugging Face API.
    /// </summary>
    /// <param name="repoId">The ID of the repository.</param>
    /// <param name="outputDir">The directory to save the downloaded files.</param>
    /// <param name="useSubDir">
    /// A flag indicating whether to create a subdirectory within the output directory 
    /// for the repository files. If set to <c>true</c>, a new subdirectory named after the repository 
    /// will be created under the specified output directory.
    /// </param>
    /// <param name="maxConcurrent">download tasks to run concurrently. The default value is 5.</param>
    /// <param name="updateInterval">interval in milliseconds to update the download progress. The default value is 100 milliseconds.</param>
    /// <returns>An asynchronous enumerable of <see cref="RepoDownloadProgress"/> objects.</returns>
    public async IAsyncEnumerable<RepoDownloadProgress> DownloadRepoAsync(
        string repoId,
        string outputDir,
        bool useSubDir = true,
        int maxConcurrent = 5,
        int updateInterval = 100,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (useSubDir)
        {
            var dirName = repoId.Replace('/', '_'); // Replace '/' with '_' to avoid invalid directory names
            outputDir = Path.Combine(outputDir, dirName);
        }
        EnsureDirectory(outputDir);

        var requestUri = new UriBuilder
        {
            Scheme = "https",
            Host = Constants.HF_HOST,            
            Path = $"{Constants.HF_GET_MODELS_PATH}/{repoId}",
        }.ToString();

        var response = await _client.GetFromJsonAsync<HuggingFaceModel>(requestUri, cancellationToken);
        var files = response?.GetFilePaths() ?? [];

        var progress = new RepoDownloadProgress
        {
            TotalFiles = files,
        };

        var downloadProgresses = new ConcurrentDictionary<string, FileDownloadProgress>();
        var semaphore = new SemaphoreSlim(maxConcurrent); // Allow a maximum of 5 concurrent downloads
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
                // Additional error handling logic required
                Console.Error.WriteLine($"Error occurred while downloading file: {file}, Error: {ex.Message}");
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        while (!tasks.All(t => t.IsCompleted))
        {
            progress.CurrentProgresses = [.. downloadProgresses.Values];
            yield return progress;
            await Task.Delay(updateInterval, cancellationToken); // Update progress every interval
        }
        await Task.WhenAll(tasks); // Ensure all download tasks are completed

        progress.IsCompleted = true;
        progress.CurrentProgresses = [.. downloadProgresses.Values];
        yield return progress;
    }

    private static HttpClient CreateHttpClient(string? token)
    {
        var client = new HttpClient();
        if (!string.IsNullOrWhiteSpace(token) && token.StartsWith("hf_"))
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }
        return client;
    }

    private static void EnsureDirectory(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(directory))
        {
            throw new InvalidOperationException($"Invalid directory path: {path}");
        }
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
