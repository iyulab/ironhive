namespace Raggle.Helpers.HuggingFace;

public class RepoDownloadProgress
{
    /// <summary>
    /// a value indicating whether the download is completed.
    /// </summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// the total file paths to download.
    /// </summary>
    public IEnumerable<string> TotalFiles { get; set; } = [];

    /// <summary>
    /// Gets the file paths that have not been downloaded.
    /// </summary>
    public IEnumerable<string> RemainingFiles => TotalFiles.Except(CompletedFiles);

    /// <summary>
    /// the file paths that have been downloaded.
    /// </summary>
    public ICollection<string> CompletedFiles { get; set; } = [];

    /// <summary>
    /// the current download tasks.
    /// </summary>
    public IEnumerable<FileDownloadProgress> CurrentProgresses { get; set; } = [];

    /// <summary>
    /// Gets the total progress of the download,
    /// Returns a value between 0 and 1.
    /// </summary>
    public double TotalProgress
    {
        get
        {
            if (TotalFiles.Any())
            {
                var completedProgress = (double)CompletedFiles.Count / TotalFiles.Count();
                var currentProgress = CurrentProgresses.Sum(progress => progress.DownloadProgress ?? 0) / TotalFiles.Count();
                return completedProgress + currentProgress;
            }
            else
            {
                return 0;
            }
        }
    }
}
