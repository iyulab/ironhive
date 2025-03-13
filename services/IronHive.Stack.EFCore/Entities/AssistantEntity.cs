using IronHive.Abstractions.ChatCompletion;
using System.Text.Json.Serialization;

namespace IronHive.Stack.EFCore.Entities;

public class AssistantEntity : ChatCompletionOptions
{
    public string Id { get; set; } = $"{Guid.NewGuid():N}";

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Instruction { get; set; }

    public IDictionary<string, object>? ToolOptions { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }

    [JsonIgnore]
    public string? ServiceId { get; set; }
}
