namespace WebApiSample.Entities;

public class MemoryCollection
{
    public string Id { get; set; } = string.Empty;

    public string CollectionName { get; set; } = string.Empty;

    public string EmbedProvider { get; set; } = string.Empty;

    public string EmbedModel { get; set; } = string.Empty;


    // Navigation
    public IEnumerable<MemoryFile>? Files { get; set; }
}
