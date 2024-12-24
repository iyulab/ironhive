using Raggle.Abstractions.Assistant;
using System.Text.Json.Serialization;

namespace Raggle.Server.Entities;

public class AssistantEntity
{
    public string Id { get; set; } = $"{Guid.NewGuid():N}";

    public string Service { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Instruction { get; set; }

    public ExecuteOptions? Options { get; set; }

    public IEnumerable<string>? Tools { get; set; }

    public IDictionary<string, object>? ToolOptions { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }

    [JsonIgnore]
    public string? ServiceId { get; set; }
}
