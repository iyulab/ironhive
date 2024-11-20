using Raggle.Abstractions.Tools;

namespace Raggle.Abstractions.Assistant;

public class RaggleAssistant
{
    public required Guid Id { get; set; } = Guid.NewGuid();

    public required string Name { get; set; }

    public string? Description { get; set; }

    public required string Provider { get; set; }

    public required string Model { get; set; }

    public string? Instructions { get; set; }

    public ToolKit[]? ToolKitList { get; set; }

    public int? MaxTokens { get; set; }

    public float? Temperature { get; set; }

    public int? TopK { get; set; }

    public float? TopP { get; set; }

    public string[]? StopSequences { get; set; }
}
