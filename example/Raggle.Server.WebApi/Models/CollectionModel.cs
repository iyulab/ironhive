using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raggle.Server.WebApi.Models;

[Table("collections")]
[Index(nameof(Id), IsUnique = true)]
public class CollectionModel
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public required string Name { get; set; }

    public string? Description { get; set; }

    [Required]
    public required RaggleServiceKeys EmbedProvider { get; set; }

    [Required]
    public required string EmbedModel { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }
}
