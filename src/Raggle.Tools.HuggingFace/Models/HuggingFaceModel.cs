using System.Text.Json;

namespace Raggle.Tools.ModelSearch.Models;

/// <summary>
/// Represents a HuggingFace model.
/// </summary>
public class HuggingFaceModel
{
    /// <summary>
    /// the Repository ID
    /// </summary>
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// the model ID.
    /// </summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// the author of the model.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// the number of downloads for the model.
    /// </summary>
    public long Downloads { get; set; }

    /// <summary>
    /// the creation date of the model.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// the last modified date of the model.
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Represents the collection of files associated with the model,
    /// such as configuration files, tokenizer files, and other necessary components
    /// that are stored in the HuggingFace repository.
    /// </summary>
    public IEnumerable<JsonElement> Siblings { get; set; } = [];

    /// <summary>
    /// Get the file paths for the model with the specified format.
    /// </summary>
    /// <param name="format">The file extension name.</param>
    /// <returns>An array of file paths.</returns>
    public string[] GetFilePaths(string? format = null)
    {
        var extension = !string.IsNullOrEmpty(format) ? $".{format}" : string.Empty;
        return Siblings
            .Where(sibling => sibling.ValueKind == JsonValueKind.Object
                              && sibling.TryGetProperty("rfilename", out var filename)
                              && filename.GetString()?.EndsWith(extension) == true)
            .Select(sibling => sibling.GetProperty("rfilename").GetString()!)
            .ToArray();
    }
}
