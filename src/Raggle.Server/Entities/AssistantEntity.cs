using System.ComponentModel.DataAnnotations;

namespace Raggle.Server.Entities;

public class AssistantEntity
{
    // Primary Key
    public string AssistantId { get; set; } = $"{Guid.NewGuid():N}";

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Instruction { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }

    public IEnumerable<string>? Memories { get; set; }

    public IEnumerable<string>? ToolKits { get; set; }

    public IDictionary<string, object>? ToolkitOptions { get; set; }
}
