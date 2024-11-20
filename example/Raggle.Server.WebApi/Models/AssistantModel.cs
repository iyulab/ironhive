using System.ComponentModel.DataAnnotations;

namespace Raggle.Server.WebApi.Models;

public class AssistantModel
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public required string Name { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    [StringLength(1000, ErrorMessage = "Instruction cannot exceed 1000 characters.")]
    public string? Instruction { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ConcurrencyCheck]
    public DateTime? LastUpdatedAt { get; set; }
}
