using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Raggle.Server.Entities;

public class DocumentEntity
{
    // Primary Key
    public string DocumentId { get; set; } = $"{Guid.NewGuid():N}";

    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public long FileSize { get; set; } = 0;

    [Required]
    public string ContentType { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }

    public IEnumerable<string>? Tags { get; set; }

    // Foreign Key
    public string CollectionId { get; set; } = string.Empty;

    // 네비게이션 속성
    [ForeignKey(nameof(CollectionId))]
    [JsonIgnore]
    public CollectionEntity? Collection { get; set; }
}
