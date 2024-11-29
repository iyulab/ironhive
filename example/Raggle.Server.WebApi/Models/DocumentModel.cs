using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Raggle.Server.WebApi.Models;

[Table("Document")]
public class DocumentModel
{
    // Primary Key
    public Guid DocumentId { get; set; } = Guid.NewGuid();

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
    public Guid CollectionId { get; set; }

    // 네비게이션 속성
    [ForeignKey(nameof(CollectionId))]
    [JsonIgnore]
    public CollectionModel? Collection { get; set; }
}
