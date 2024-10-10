using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Web;

namespace Raggle.Helpers.HuggingFace;

/// <summary>
/// a search and download client for interacting with the Hugging Face API.
/// </summary>
public class HuggingFaceClient : IDisposable
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
    public async Task<HuggingFaceModel[]> SearchModelsAsync(
        string? search = null,
        string[]? filters = null,
        int limit = 5,
        string sort = "downloads",
        bool descending = true,
        CancellationToken cancellationToken = default)
    {
        var query = HttpUtility.ParseQueryString(HuggingFaceConstants.GetModelsDefaultQuery);
        if (!string.IsNullOrWhiteSpace(search))
            query["search"] = search;
        if (filters != null && filters.Length > 0)
            query["filter"] = string.Join(",", filters.Select(filter => filter.Trim()));
        if (limit > 0)
            query["limit"] = limit.ToString();
        if (!string.IsNullOrWhiteSpace(sort))
            query["sort"] = sort;
        query["direction"] = descending ? "-1" : "1";

        var requestUri = new UriBuilder
        {
            Scheme = "https",
            Host = HuggingFaceConstants.Host,
            Path = HuggingFaceConstants.GetModelsPath,
            Query = query.ToString()
        }.ToString();

        var models = await _client.GetFromJsonAsync<HuggingFaceModel[]>(requestUri, cancellationToken);
        return models ?? [];
    }

    public async Task<HuggingFaceModel> FindModelByRepoIdAsync(
        string repoId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(repoId))
            throw new ArgumentException("Repository ID cannot be null or whitespace.", nameof(repoId));

        // Construct the request URI for the specific model
        var requestUri = new UriBuilder
        {
            Scheme = "https",
            Host = HuggingFaceConstants.Host,
            Path = $"{HuggingFaceConstants.GetModelsPath}/{Uri.EscapeDataString(repoId)}"
        }.ToString();

        try
        {
            // Attempt to retrieve the model
            var model = await _client.GetFromJsonAsync<HuggingFaceModel>(requestUri, cancellationToken);

            if (model == null)
            {
                throw new InvalidOperationException($"Model with repoId '{repoId}' was not found.");
            }

            return model;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException($"Model with repoId '{repoId}' does not exist.", ex);
        }
    }

    /// <summary>
    /// Retrieves metadata information about a specific file in a Hugging Face repository
    /// </summary>
    /// <param name="repoId">
    /// The ID of the repository (e.g., "username/repo-name").
    /// </param>
    /// <param name="filePath">
    /// The path of the file within the repository (e.g., "models/model.bin").
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="HuggingFaceFile"/> object
    /// with metadata information about the specified file.
    /// </returns>
    public async Task<HuggingFaceFile> GetFileInfoAsync(
        string repoId,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var requestUri = new UriBuilder
        {
            Scheme = "https",
            Host = HuggingFaceConstants.Host,
            Path = string.Format(HuggingFaceConstants.GetFilePath, repoId, filePath),
            Query = HuggingFaceConstants.GetFileDefaultQuery
        }.ToString();

        // Attempt to use HEAD request to get headers without downloading the body
        var request = new HttpRequestMessage(HttpMethod.Head, requestUri);

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var fileInfo = new HuggingFaceFile
        {
            Name = Path.GetFileName(filePath),
            Path = filePath,
            Size = response.Content.Headers.ContentLength,
            MimeType = response.Content.Headers.ContentType?.MediaType,
            LastModified = response.Content.Headers.LastModified?.UtcDateTime
        };

        if (IsTextMimeType(fileInfo.MimeType))
        {
            var getRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);

            using var getResponse = await _client.SendAsync(getRequest, HttpCompletionOption.ResponseContentRead, cancellationToken);
            getResponse.EnsureSuccessStatusCode();

            fileInfo.Content = await getResponse.Content.ReadAsStringAsync(cancellationToken);
        }

        return fileInfo;
    }

    /// <summary>
    /// Downloads a file from the Hugging Face API.
    /// </summary>
    /// <param name="repoId">
    /// The ID of the repository.
    /// </param>
    /// <param name="filePath">
    /// The path of the file in the repository.
    /// </param>
    /// <param name="outputPath">
    /// The path to save the downloaded file.
    /// if the output path is a directory, the file will be saved with the same name as the original file in the directory.
    /// else, the file will be saved with the specified output path.
    /// </param>
    /// <param name="startFrom">
    /// The byte offset to start downloading the file from. The default value is 0.
    /// </param>
    /// <returns>
    /// An asynchronous enumerable of <see cref="FileDownloadProgress"/> objects.
    /// </returns>
    public async IAsyncEnumerable<FileDownloadProgress> DownloadFileAsync(
        string repoId,
        string filePath,
        string outputPath,
        long startFrom = 0,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var requestUri = new UriBuilder
        {
            Scheme = "https",
            Host = HuggingFaceConstants.Host,
            Path = string.Format(HuggingFaceConstants.GetFilePath, repoId, filePath),
            Query = HuggingFaceConstants.GetFileDefaultQuery
        }.ToString();

        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        if (startFrom > 0)
        {
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(startFrom, null);
        }

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength.HasValue
            ? response.Content.Headers.ContentLength.Value + startFrom
            : (long?)null;

        if (!Path.HasExtension(outputPath))
        {
            outputPath = Path.Combine(outputPath, Path.GetFileName(filePath));
        }
        EnsureDirectory(outputPath);

        using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var fileStream = new FileStream(
            outputPath,
            startFrom > 0 ? FileMode.Append : FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            8192,
            true);

        var buffer = new byte[8192];
        long totalBytesRead = startFrom;
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
            var downloadSpeed = elapsedTime.TotalSeconds > 0 ? totalBytesRead / elapsedTime.TotalSeconds : 0;

            progress.CurrentBytes = totalBytesRead;
            progress.DownloadSpeed = downloadSpeed;
            if (totalBytes.HasValue && downloadSpeed > 0)
            {
                progress.DownloadProgress = (double)totalBytesRead / totalBytes.Value;
                progress.RemainingTime = TimeSpan.FromSeconds((totalBytes.Value - totalBytesRead) / downloadSpeed);
            }
            yield return progress;
        }

        progress.IsCompleted = true;
        progress.DownloadSpeed = 0;
        progress.DownloadProgress = 1;
        progress.RemainingTime = TimeSpan.Zero;
        yield return progress;
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Private Methods

    private static HttpClient CreateHttpClient(string? token)
    {
        var client = new HttpClient();
        if (!string.IsNullOrWhiteSpace(token) && token.StartsWith("hf_"))
        {
            client.DefaultRequestHeaders.Add(HuggingFaceConstants.AuthHeaderName, 
                string.Format(HuggingFaceConstants.AuthHeaderValue, token));
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

    private bool IsTextMimeType(string? mimeType)
    {
        if (string.IsNullOrEmpty(mimeType))
            return false;

        return mimeType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ||
               mimeType.Equals("application/json", StringComparison.OrdinalIgnoreCase) ||
               mimeType.Equals("application/xml", StringComparison.OrdinalIgnoreCase) ||
               mimeType.Equals("application/javascript", StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Temporal Methods

    /// <summary>
    /// Downloads a repository from the Hugging Face API.
    /// </summary>
    /// <remarks>
    /// ** 현재 객체에 적합하지 않은 메서드로 판단됩니다. **
    /// </remarks>
    /// <param name="repoId">
    /// The ID of the repository.
    /// </param>
    /// <param name="outputDir">
    /// The directory to save the downloaded files.
    /// </param>
    /// <param name="useSubDir">
    /// A flag indicating whether to create a subdirectory within the output directory 
    /// for the repository files. If set to <c>true</c>, a new subdirectory named after the repository 
    /// will be created under the specified output directory.
    /// </param>
    /// <param name="maxConcurrent">
    /// download tasks to run concurrently. The default value is 5.
    /// </param>
    /// <param name="updateInterval">
    /// interval in milliseconds to update the download progress. The default value is 100 milliseconds.
    /// </param>
    /// <returns>
    /// An asynchronous enumerable of <see cref="RepoDownloadProgress"/> objects.
    /// </returns>
    [Obsolete("DownloadRepoAsync is experimental and may change or be removed in future")]
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
            Host = HuggingFaceConstants.Host,
            Path = string.Format(HuggingFaceConstants.GetModelPath, repoId)
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
                await foreach (var fileProgress in DownloadFileAsync(repoId, file, outputPath, cancellationToken: cancellationToken))
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

    #endregion
}
