namespace Raggle.Server.Models;

public class Assistant
{
    public required Guid ID { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string Instruction { get; set; }
    public required IEnumerable<string> Tools { get; set; } = [];
    public required IEnumerable<Guid> Knowledes { get; set; } = [];
}
