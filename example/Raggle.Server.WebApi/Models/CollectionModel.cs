using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raggle.Server.WebApi.Models;

[Table("collections")]
[Index(nameof(Id), IsUnique = true)]
public class CollectionModel
{
    [Key]
    public required Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public required string Name { get; set; }

    public string? Description { get; set; }

    [Required]
    public required string EmbeddingModel { get; set; }

    [Required]
    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }
}
