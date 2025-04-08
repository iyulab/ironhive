namespace WebApiSample.Data;

public enum VectorizationStatus
{
    Queued,
    Processing,
    Completed,
    Failed
}

public class FileEntity
{
    public string Id { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string EmbeddingModel { get; set; } = string.Empty;

    public VectorizationStatus Status { get; set; } = VectorizationStatus.Queued;

    public string? StatusMessage { get; set; }

    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}
