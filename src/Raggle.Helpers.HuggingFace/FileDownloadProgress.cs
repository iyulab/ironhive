namespace Raggle.Helpers.HuggingFace;

/// <summary>
/// information about the progress of a file download.
/// </summary>
public class FileDownloadProgress
{
    /// <summary>
    /// a value indicating whether the file download is completed.
    /// </summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// the path where the file is uploaded.
    /// </summary>
    public string UploadPath { get; set; } = string.Empty;

    /// <summary>
    /// the current number of bytes downloaded.
    /// </summary>
    public long CurrentBytes { get; set; }

    /// <summary>
    /// the total number of bytes to be downloaded.
    /// </summary>
    public long? TotalBytes { get; set; }

    /// <summary>
    /// the download speed in bytes per second.
    /// </summary>
    public double DownloadSpeed { get; set; }

    /// <summary>
    /// the remaining time in seconds.
    /// </summary>
    public TimeSpan? RemainingTime { get; set; }

    /// <summary>
    /// the download progress from 0.0 to 1.0.
    /// </summary>
    public double? DownloadProgress { get; set; }

    /// <summary>
    /// Gets the formatted current size of the downloaded file.
    /// </summary>
    public string FormattedCurrentSize => StringFormatter.FormatSize(CurrentBytes);

    /// <summary>
    /// Gets the formatted total size of the file to be downloaded.
    /// </summary>
    public string FormattedTotalSize => TotalBytes.HasValue ? StringFormatter.FormatSize(TotalBytes.Value) : "Unknown";

    /// <summary>
    /// Gets the formatted download speed in bytes per second.
    /// </summary>
    public string FormattedDownloadSpeed => StringFormatter.FormatSpeed(DownloadSpeed);

    /// <summary>
    /// Gets the formatted remaining time in hours:minutes:seconds format.
    /// </summary>
    public string FormattedRemainingTime => RemainingTime.HasValue ? StringFormatter.FormatTimeSpan(RemainingTime.Value) : "Unknown";

    /// <summary>
    /// Gets the formatted download progress as a percentage.
    /// </summary>
    public string FormattedProgress => DownloadProgress.HasValue ? StringFormatter.FormatProgress(DownloadProgress.Value) : "Unknown";

    /// <summary>
    /// Returns a string representation of the FileDownloadProgress object.
    /// </summary>
    /// <returns>A string representation of the object.</returns>
    public override string ToString()
    {
        return $"""
                Download Status: {(IsCompleted ? "Completed" : "In Progress")}
                Current Size: {FormattedCurrentSize}, Total Size: {FormattedTotalSize}
                Download Speed: {FormattedDownloadSpeed}
                Progress: {FormattedProgress}
                Remaining Time: {FormattedRemainingTime}
                Upload Path: {UploadPath}
                """;
    }

}
