using Raggle.Abstractions.AI;
using Raggle.Abstractions.Assistant;

namespace Raggle.Server.Entities;

public class AssistantEntity
{
    public string Id { get; set; } = $"{Guid.NewGuid():N}";

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Instruction { get; set; }

    public required AssistantOptions Options { get; set; }

    public IEnumerable<string>? Tools { get; set; }

    public IDictionary<string, object>? ToolOptions { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }
}
