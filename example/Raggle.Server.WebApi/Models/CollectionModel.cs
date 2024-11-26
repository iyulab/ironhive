using Microsoft.EntityFrameworkCore;
using Raggle.Server.WebApi.Configuration;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raggle.Server.WebApi.Models;

[Table("Collections")]
[Index(nameof(Id), IsUnique = true)]
public class CollectionModel
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public AIServiceKeys EmbedProvider { get; set; }

    [Required]
    public string EmbedModel { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }
}
