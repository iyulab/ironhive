using System.Text.Json.Serialization;

namespace Raggle.Server.Entities;

public class CollectionEntity
{
    public string Id { get; set; } = $"c_{Guid.NewGuid():N}";

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string EmbedService { get; set; } = string.Empty;

    public string EmbedModel { get; set; } = string.Empty;

    public IDictionary<string, object>? HandlerOptions { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }

    // 네비게이션 속성
    [JsonIgnore]
    public ICollection<DocumentEntity>? Documents { get; set; }

    [JsonIgnore]
    public string? ServiceId { get; set; }
}
