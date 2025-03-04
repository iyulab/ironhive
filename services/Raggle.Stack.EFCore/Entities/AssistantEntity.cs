using Raggle.Abstractions.ChatCompletion;
using System.Text.Json.Serialization;

namespace Raggle.Stack.EFCore.Entities;

public class AssistantEntity : ChatCompletionOptions
{
    public string Id { get; set; } = $"{Guid.NewGuid():N}";

    public string Model { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Instruction { get; set; }

    public IEnumerable<string>? Tools { get; set; }

    public IDictionary<string, object>? ToolOptions { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }

    [JsonIgnore]
    public string? ServiceId { get; set; }
}
