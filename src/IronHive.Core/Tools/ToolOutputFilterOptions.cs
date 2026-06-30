namespace IronHive.Core.Tools;

/// <summary>
/// Configuration options for <see cref="ToolOutputFilter"/>.
/// </summary>
public class ToolOutputFilterOptions
{
    /// <summary>
    /// Whether to convert JSON array outputs to CSV format.
    /// JSON arrays of flat objects are converted to CSV for ~40-50% token savings.
    /// Default: true.
    /// </summary>
    public bool EnableJsonToCsv { get; set; } = true;

    /// <summary>
    /// Minimum number of array elements required for JSON→CSV conversion.
    /// Arrays with fewer elements are not worth converting.
    /// Default: 3.
    /// </summary>
    public int JsonToCsvMinElements { get; set; } = 3;

    /// <summary>
    /// Whether to normalize excessive whitespace in outputs.
    /// Collapses 3+ consecutive blank lines to 2, trims trailing whitespace.
    /// Default: true.
    /// </summary>
    public bool EnableWhitespaceNormalization { get; set; } = true;

    /// <summary>
    /// Maximum output character count before truncation.
    /// Outputs exceeding this limit are truncated with head+tail preservation.
    /// Default: 50,000.
    /// </summary>
    public int MaxResultChars { get; set; } = 50_000;

    /// <summary>
    /// Number of lines to keep from the beginning when truncating.
    /// Default: 100.
    /// </summary>
    public int KeepHeadLines { get; set; } = 100;

    /// <summary>
    /// Number of lines to keep from the end when truncating.
    /// Default: 30.
    /// </summary>
    public int KeepTailLines { get; set; } = 30;
}
