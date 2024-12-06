namespace Raggle.Server.Entities;

public class AssistantEntity
{
    // Primary Key
    public string AssistantId { get; set; } = $"{Guid.NewGuid():N}";

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Instruction { get; set; }

    public ServiceSettings Settings { get; set; } = new ServiceSettings();

    public IEnumerable<string>? Memories { get; set; }

    public IEnumerable<string>? ToolKits { get; set; }

    public IDictionary<string, object>? ToolkitOptions { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }
}

public class ServiceSettings
{
    public string Provider { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int MaxTokens { get; set; } = 2048;

    public float Temperature { get; set; } = 0.7f;

    public int TopK { get; set; } = 50;

    public float TopP { get; set; } = 1.0f;

    public string[]? StopSequences { get; set; }
}
