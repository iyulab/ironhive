using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Raggle.Server.Entities;

public class DocumentEntity
{
    public string Id { get; set; } = $"{Guid.NewGuid():N}";

    public string FileName { get; set; } = string.Empty;

    public long FileSize { get; set; } = 0;

    public string ContentType { get; set; } = string.Empty;

    public IEnumerable<string>? Tags { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }

    public string CollectionId { get; set; } = string.Empty;

    // 네비게이션 속성
    [JsonIgnore]
    public CollectionEntity? Collection { get; set; }
}
