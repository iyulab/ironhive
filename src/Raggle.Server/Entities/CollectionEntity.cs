using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Raggle.Server.Entities;

public class CollectionEntity
{
    // Primary Key
    public string CollectionId { get; set; } = $"c_{Guid.NewGuid():N}";

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public string EmbedServiceKey { get; set; } = string.Empty;

    [Required]
    public string EmbedModelName { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }

    public IDictionary<string, object>? HandlerOptions { get; set; }

    // 네비게이션 속성
    [JsonIgnore]
    public ICollection<DocumentEntity> Documents { get; set; } = [];
}
