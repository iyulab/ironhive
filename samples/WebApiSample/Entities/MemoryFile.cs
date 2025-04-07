namespace WebApiSample.Entities;

public class MemoryFile
{
    public string Id { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public string? CollectionId { get; set; }
}
