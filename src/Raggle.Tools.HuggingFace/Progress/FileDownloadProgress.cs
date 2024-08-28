namespace Raggle.Tools.ModelSearch.Progress;

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
    public string FormattedCurrentSize => GetFormattedSize(CurrentBytes);

    /// <summary>
    /// Gets the formatted total size of the file to be downloaded.
    /// </summary>
    public string FormattedTotalSize => TotalBytes.HasValue ? GetFormattedSize(TotalBytes.Value) : "Unknown size";

    /// <summary>
    /// Gets the formatted download speed in bytes per second.
    /// </summary>
    public string FormattedDownloadSpeed => GetFormattedSpeed(DownloadSpeed);

    /// <summary>
    /// Gets the formatted remaining time in hours:minutes:seconds format.
    /// </summary>
    public string FormattedRemainingTime => RemainingTime.HasValue ? GetFormattedTime(RemainingTime.Value) : "Unknown time";

    /// <summary>
    /// Gets the formatted download progress as a percentage.
    /// </summary>
    public string FormattedProgress => DownloadProgress.HasValue ? GetFormattedProgress(DownloadProgress.Value) : "Unknown progress";

    /// <summary>
    /// Formats the size in bytes to a human-readable format.
    /// </summary>
    /// <param name="bytes">The size in bytes.</param>
    /// <param name="decimalPlaces">The number of decimal places to round the result to. Default is 2.</param>
    /// <returns>The formatted size string.</returns>
    public static string GetFormattedSize(long bytes, int decimalPlaces = 2)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        string formatString = $"{{0:0.{new string('#', decimalPlaces)}}} {{1}}";
        return string.Format(formatString, len, sizes[order]);
    }

    /// <summary>
    /// Formats the time span to a string in the specified format.
    /// </summary>
    /// <param name="timeSpan">The time span to format.</param>
    /// <param name="format">The format string. Default is "hh\\:mm\\:ss".</param>
    /// <returns>The formatted time string.</returns>
    public static string GetFormattedTime(TimeSpan timeSpan, string format = "hh\\:mm\\:ss")
    {
        return timeSpan.ToString(format);
    }

    /// <summary>
    /// Formats the download speed in bytes per second to a human-readable format.
    /// </summary>
    /// <param name="bytesPerSecond">The download speed in bytes per second.</param>
    /// <param name="decimalPlaces">The number of decimal places to round the result to. Default is 2.</param>
    /// <returns>The formatted download speed string.</returns>
    public static string GetFormattedSpeed(double bytesPerSecond, int decimalPlaces = 2)
    {
        return GetFormattedSize((long)bytesPerSecond, decimalPlaces) + "/s";
    }

    /// <summary>
    /// Formats the download progress as a percentage string.
    /// </summary>
    /// <param name="downloadProgress">The download progress from 0.0 to 1.0.</param>
    /// <returns>The formatted download progress string.</returns>
    public static string GetFormattedProgress(double downloadProgress)
    {
        int percentage = (int)(downloadProgress * 100);
        return $"{percentage}%";
    }

    /// <summary>
    /// Returns a string representation of the FileDownloadProgress object.
    /// </summary>
    /// <returns>A string representation of the object.</returns>
    public override string ToString()
    {
        var progressString = DownloadProgress.HasValue ? $"{DownloadProgress.Value * 100:0.00}%" : "Unknown progress";
        var remainingTimeString = RemainingTime.HasValue ? FormattedRemainingTime : "Unknown time";

        return $"Download Status: {(IsCompleted ? "Completed" : "In Progress")}\n" +
               $"Current Size: {FormattedCurrentSize}, Total Size: {FormattedTotalSize}\n" +
               $"Download Speed: {FormattedDownloadSpeed}\n" +
               $"Progress: {progressString}\n" +
               $"Remaining Time: {remainingTimeString}\n" +
               $"Upload Path: {UploadPath}";
    }

}
