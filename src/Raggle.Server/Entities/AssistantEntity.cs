namespace Raggle.Server.Entities;

public class AssistantEntity
{
    // Primary Key
    public string AssistantId { get; set; } = $"{Guid.NewGuid():N}";

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Instruction { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int? MaxTokens { get; set; }

    public float? Temperature { get; set; }

    public int? TopK { get; set; }

    public float? TopP { get; set; }

    public string[]? StopSequences { get; set; }

    public IEnumerable<string>? Memories { get; set; }

    public IEnumerable<string>? Tools { get; set; }

    public IDictionary<string, object>? ToolOptions { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }
}
