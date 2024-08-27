namespace Raggle.Tools.HuggingFace;

public class RepoDownloadProgress
{
    public bool IsCompleted { get; set; } = false;

    // The total file paths to download
    public string[] TotalFiles { get; set; } = [];

    // The file paths that have not been downloaded
    public string[] RemainingFiles => TotalFiles.Except(CompletedFiles).ToArray();

    // The file paths that have been downloaded
    public ICollection<string> CompletedFiles { get; set; } = [];

    // The current download tasks
    public ICollection<FileDownloadProgress> CurrentProgresses { get; set; } = [];

    public double TotalProgress
    {
        get
        {
            if (TotalFiles.Length > 0)
            {
                var completedProgress = (double)CompletedFiles.Count / TotalFiles.Length;
                var currentProgress = CurrentProgresses.Sum(progress => progress.DownloadProgress ?? 0) / TotalFiles.Length;
                return completedProgress + currentProgress;
            }
            else
            {
                return 0;
            }
        }
    }
}
