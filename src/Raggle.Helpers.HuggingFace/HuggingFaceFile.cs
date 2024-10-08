namespace Raggle.Helpers.HuggingFace;

/// <summary>
/// Represents a file information in the Hugging Face model.
/// </summary>
public class HuggingFaceFile
{
    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the path of the file in the repository.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the size of the file.
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// Gets the formatted size of the file.
    /// </summary>
    public string FormattedSize => Size.HasValue ? StringFormatter.FormatSize(Size.Value) : "Unknown";

    /// <summary>
    /// Gets or sets the type of the file.
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// Gets or sets the last modified date of the file.
    /// </summary>
    public DateTime? LastModified { get; set; }

    //public IDictionary<string, object>? Metadata { get; set; }
}
