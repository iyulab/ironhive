using System.Globalization;

namespace Raggle.Helpers.HuggingFace;

/// <summary>
/// Provides utility methods for formatting various types of strings.
/// </summary>
public static class StringFormatter
{
    /// <summary>
    /// Formats the size in bytes to a human-readable string with appropriate units.
    /// </summary>
    /// <param name="bytes">The size in bytes.</param>
    /// <param name="decimalPlaces">Number of decimal places. Default is 0</param>
    /// <returns>Formatted size string.</returns>
    public static string FormatSize(long bytes, int decimalPlaces = 0)
    {
        if (bytes < 0)
            throw new ArgumentOutOfRangeException(nameof(bytes), "Bytes cannot be negative.");

        string[] sizes = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len.ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture)} {sizes[order]}";
    }

    /// <summary>
    /// Formats a TimeSpan to a string in the specified format.
    /// </summary>
    /// <param name="timeSpan">The TimeSpan to format.</param>
    /// <param name="format">The format string. Default is "hh\\:mm\\:ss".</param>
    /// <returns>Formatted time string.</returns>
    public static string FormatTimeSpan(TimeSpan timeSpan, string format = @"hh\:mm\:ss")
    {
        if (timeSpan.TotalSeconds < 0)
            return "Unknown";

        return timeSpan.ToString(format, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats the download speed in bytes per second to a human-readable string.
    /// </summary>
    /// <param name="bytesPerSecond">Download speed in bytes per second.</param>
    /// <param name="decimalPlaces">Number of decimal places. Default is 0</param>
    /// <returns>Formatted download speed string.</returns>
    public static string FormatSpeed(double bytesPerSecond, int decimalPlaces = 0)
    {
        if (bytesPerSecond < 0)
            throw new ArgumentOutOfRangeException(nameof(bytesPerSecond), "Speed cannot be negative.");

        string[] sizes = ["B/s", "KB/s", "MB/s", "GB/s", "TB/s", "PB/s", "EB/s"];
        double speed = bytesPerSecond;
        int order = 0;

        while (speed >= 1024 && order < sizes.Length - 1)
        {
            order++;
            speed /= 1024;
        }

        return $"{speed.ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture)} {sizes[order]}";
    }

    /// <summary>
    /// Formats the download progress as a percentage string with specified decimal places.
    /// </summary>
    /// <param name="downloadProgress">Download progress from 0.0 to 1.0.</param>
    /// <param name="decimalPlaces">Number of decimal places. Default is 0</param>
    /// <returns>Formatted download progress string.</returns>
    public static string FormatProgress(double downloadProgress, int decimalPlaces = 0)
    {
        if (downloadProgress < 0 || downloadProgress > 1)
            throw new ArgumentOutOfRangeException(nameof(downloadProgress), "Download progress must be between 0.0 and 1.0.");

        string format = $"F{decimalPlaces}";
        return $"{(downloadProgress * 100).ToString(format, CultureInfo.InvariantCulture)}%";
    }
}
