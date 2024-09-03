namespace Raggle.Abstractions.Models;

public class JsonSchema
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public object? Parameters { get; set; }
    public IEnumerable<string>? Required { get; set; }
}
